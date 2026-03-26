using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Lombiq.Marketing.UrlShortener.Events;
using Lombiq.Marketing.UrlShortener.Models;
using Lombiq.Marketing.UrlShortener.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Lombiq.Marketing.UrlShortener.Middlewares;

public sealed class ShortUrlRedirectMiddleware
{
    private readonly RequestDelegate _next;

    public ShortUrlRedirectMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        IUrlShorteningService urlShorteningService,
        IEnumerable<IShortUrlRedirectEventHandler> redirectEventHandlers)
    {
        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var shortUrl = context.Request.Path.Value;
        if (string.IsNullOrEmpty(shortUrl) || shortUrl == "/" || context.IsAdminUrl())
        {
            await _next(context);
            return;
        }

        var redirectInfo = await urlShorteningService.GetRedirectInfoAsync(shortUrl);
        if (string.IsNullOrEmpty(redirectInfo?.DestinationUrl))
        {
            await _next(context);
            return;
        }

        var redirectContext = new ShortUrlRedirectContext
        {
            HttpContext = context,
            ShortUrl = shortUrl,
            DestinationUrl = BuildRedirectUrl(redirectInfo!),
        };

        foreach (var redirectEventHandler in redirectEventHandlers)
        {
            await redirectEventHandler.RedirectingAsync(redirectContext);
        }

        if (redirectContext.Cancel || string.IsNullOrEmpty(redirectContext.DestinationUrl))
        {
            await _next(context);
            return;
        }

        // This redirect comes from user input, but we assume that the user is the site owner or a user we trust.
        // It might be a good idea to add a whitelist of allowed domains for absolute URLs in the future.
#pragma warning disable SCS0027 // SCS0027: Potential Open Redirect vulnerability was found
        context.Response.Redirect(redirectContext.DestinationUrl, permanent: false);
#pragma warning restore SCS0027
    }

    private static string BuildRedirectUrl(ShortUrlRedirectInfo redirectInfo)
    {
        if (Uri.TryCreate(redirectInfo.DestinationUrl, UriKind.RelativeOrAbsolute, out var destinationUri) && destinationUri.IsAbsoluteUri)
        {
            return BuildAbsoluteRedirectUrl(destinationUri, redirectInfo);
        }

        return BuildRelativeRedirectUrl(redirectInfo.DestinationUrl, redirectInfo);
    }

    private static string BuildAbsoluteRedirectUrl(Uri destinationUri, ShortUrlRedirectInfo redirectInfo)
    {
        var query = HttpUtility.ParseQueryString(destinationUri.Query);
        query.Add("utm_source", redirectInfo.UtmSource);
        query.Add("utm_medium", redirectInfo.UtmMedium);
        query.Add("utm_campaign", redirectInfo.UtmCampaign);
        query.Add("utm_content", redirectInfo.UtmContent);
        query.Add("utm_term", redirectInfo.UtmTerm);

        var uriBuilder = new UriBuilder(destinationUri)
        {
            Query = query.ToString() ?? string.Empty,
        };

        return uriBuilder.Uri.ToString();
    }

    private static string BuildRelativeRedirectUrl(string destinationUrl, ShortUrlRedirectInfo redirectInfo)
    {
        var fragmentStartIndex = destinationUrl.IndexOf('#');
        var fragment = fragmentStartIndex >= 0 ? destinationUrl[fragmentStartIndex..] : string.Empty;
        var destinationUrlWithoutFragment = fragmentStartIndex >= 0
            ? destinationUrl[..fragmentStartIndex]
            : destinationUrl;

        var queryStartIndex = destinationUrlWithoutFragment.IndexOf('?');
        var path = queryStartIndex >= 0
            ? destinationUrlWithoutFragment[..queryStartIndex]
            : destinationUrlWithoutFragment;
        var queryString = queryStartIndex >= 0
            ? destinationUrlWithoutFragment[queryStartIndex..]
            : string.Empty;

        var queryBuilder = BuildQueryString(CreateQueryParameters(queryString, redirectInfo));

        return $"{path}{queryBuilder}{fragment}";
    }

    private static Dictionary<string, StringValues> CreateQueryParameters(string queryString, ShortUrlRedirectInfo redirectInfo)
    {
        var queryParameters = new Dictionary<string, StringValues>(QueryHelpers.ParseQuery(queryString), StringComparer.OrdinalIgnoreCase);

        SetQueryParameter(queryParameters, "utm_source", redirectInfo.UtmSource);
        SetQueryParameter(queryParameters, "utm_medium", redirectInfo.UtmMedium);
        SetQueryParameter(queryParameters, "utm_campaign", redirectInfo.UtmCampaign);
        SetQueryParameter(queryParameters, "utm_content", redirectInfo.UtmContent);
        SetQueryParameter(queryParameters, "utm_term", redirectInfo.UtmTerm);

        return queryParameters;
    }

    private static QueryBuilder BuildQueryString(IDictionary<string, StringValues> queryParameters)
    {
        var queryBuilder = new QueryBuilder();
        foreach (var parameter in queryParameters)
        {
            foreach (var value in parameter.Value)
            {
                queryBuilder.Add(parameter.Key, value);
            }
        }

        return queryBuilder;
    }

    private static void SetQueryParameter(IDictionary<string, StringValues> queryParameters, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            queryParameters.Remove(key);
            return;
        }

        queryParameters[key] = value;
    }
}

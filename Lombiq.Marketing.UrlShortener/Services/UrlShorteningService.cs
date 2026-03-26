using Lombiq.Marketing.UrlShortener.Indexes;
using Lombiq.Marketing.UrlShortener.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Marketing.UrlShortener.Services;

public class UrlShorteningService : IUrlShorteningService
{
    private const string ShortUrlCacheKeyPrefix = "ShortUrlRedirectUrl:";

    private readonly ISession _session;
    private readonly IMemoryCache _memoryCache;

    public UrlShorteningService(ISession session, IMemoryCache memoryCache)
    {
        _session = session;
        _memoryCache = memoryCache;
    }

    public async Task<string?> GetRedirectUrlAsync(string shortUrl)
    {
        if (_memoryCache.TryGetValue(GetCacheKey(shortUrl), out string? redirectUrl))
        {
            return redirectUrl;
        }

        string? cachedRedirectUrl = null;

        if (await _session.Query<ContentItem, ContentItemIndex>(index => index.Published)
            .With<ShortUrlPartIndex>(index => index.ShortUrl == shortUrl)
            .FirstOrDefaultAsync() is { } shortUrlContentItem)
        {
            cachedRedirectUrl = SetCache(shortUrlContentItem);
        }

        return cachedRedirectUrl;
    }

    public async Task<bool> IsShortUrlUniqueAsync(ShortUrlPart shortUrlPart)
    {
        // Check if the short URL already exists in the cache. This is a quick check to avoid hitting the database if
        // we already know the short URL is taken.
        if (_memoryCache.TryGetValue(GetCacheKey(shortUrlPart.ShortUrl.Text), out string _))
        {
            return false;
        }

        // Check if the short URL already exists in the database.
        if (await _session.Query<ContentItem, ContentItemIndex>(index => index.Published)
            .With<ShortUrlPartIndex>(index =>
                index.ShortUrl == shortUrlPart.ShortUrl.Text &&
                index.ContentItemId != shortUrlPart.ContentItem.ContentItemId)
            .FirstOrDefaultAsync() is { } existingShortUrl)
        {
            // Cache the existing short URL to prevent future database hits for the same short URL.
            SetCache(existingShortUrl);
            return false;
        }

        return true;
    }

    public async Task<bool> UpdateShortUrlAsync(string? previousShortUrl, ShortUrlPart shortUrlPart)
    {
        // If the short URL is being changed, we need to check if the new short URL is unique.
        if (!string.IsNullOrEmpty(previousShortUrl) &&
            shortUrlPart.ShortUrl.Text != previousShortUrl &&
            !await IsShortUrlUniqueAsync(shortUrlPart))
        {
            return false;
        }

        SetCache(shortUrlPart);

        if (!string.IsNullOrEmpty(previousShortUrl) && shortUrlPart.ShortUrl.Text != previousShortUrl)
        {
            RemoveCache(previousShortUrl);
        }

        return true;
    }

    public Task DeleteShortUrlAsync(ContentItem shortUrlContentItem)
    {
        // Remove the short URL from the cache to ensure it doesn't return stale data after deletion.
        RemoveCache(shortUrlContentItem.As<ShortUrlPart>().ShortUrl.Text);
        return Task.CompletedTask;
    }

    private string SetCache(ContentItem shortUrlContentItem) =>
        SetCache(shortUrlContentItem.As<ShortUrlPart>());

    private string SetCache(ShortUrlPart shortUrlPart)
    {
        var redirectUrl = BuildRedirectUrl(shortUrlPart.ContentItem);
        _memoryCache.Set(GetCacheKey(shortUrlPart.ShortUrl.Text), redirectUrl);

        return redirectUrl;
    }

    private void RemoveCache(string shortUrl) =>
        _memoryCache.Remove(GetCacheKey(shortUrl));

    private static string BuildRedirectUrl(ContentItem shortUrlContentItem)
    {
        var shortUrlPart = shortUrlContentItem.As<ShortUrlPart>();
        var utmPart = shortUrlContentItem.As<UtmPart>();

        return BuildRedirectUrl(shortUrlPart.DestinationUrl.Text, utmPart);
    }

    private static string BuildRedirectUrl(string destinationUrl, UtmPart utmPart)
    {
        if (Uri.TryCreate(destinationUrl, UriKind.RelativeOrAbsolute, out var destinationUri) && destinationUri.IsAbsoluteUri)
        {
            return BuildAbsoluteRedirectUrl(destinationUri, utmPart);
        }

        return BuildRelativeRedirectUrl(destinationUrl, utmPart);
    }

    private static string BuildAbsoluteRedirectUrl(Uri destinationUri, UtmPart utmPart)
    {
        var queryParameters = CreateQueryParameters(destinationUri.Query, utmPart);
        var queryBuilder = BuildQueryString(queryParameters);

        var uriBuilder = new UriBuilder(destinationUri)
        {
            Query = queryBuilder.ToString().TrimStart('?'),
        };

        return uriBuilder.Uri.ToString();
    }

    private static string BuildRelativeRedirectUrl(string destinationUrl, UtmPart utmPart)
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

        var queryBuilder = BuildQueryString(CreateQueryParameters(queryString, utmPart));

        return $"{path}{queryBuilder}{fragment}";
    }

    private static Dictionary<string, StringValues> CreateQueryParameters(string queryString, UtmPart utmPart)
    {
        var queryParameters = new Dictionary<string, StringValues>(QueryHelpers.ParseQuery(queryString), StringComparer.OrdinalIgnoreCase);

        SetQueryParameter(queryParameters, "utm_source", utmPart.UtmSource.Text);
        SetQueryParameter(queryParameters, "utm_medium", utmPart.UtmMedium.Text);
        SetQueryParameter(queryParameters, "utm_campaign", utmPart.UtmCampaign.Text);
        SetQueryParameter(queryParameters, "utm_content", utmPart.UtmContent.Text);
        SetQueryParameter(queryParameters, "utm_term", utmPart.UtmTerm.Text);

        return queryParameters;
    }

    private static QueryBuilder BuildQueryString(IDictionary<string, StringValues> queryParameters)
    {
        var queryBuilder = new QueryBuilder();
        foreach (var parameter in queryParameters)
        {
            foreach (var value in parameter.Value.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                queryBuilder.Add(parameter.Key, value!);
            }
        }

        return queryBuilder;
    }

    private static void SetQueryParameter(Dictionary<string, StringValues> queryParameters, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            queryParameters.Remove(key);
            return;
        }

        queryParameters[key] = value;
    }

    private static string GetCacheKey(string shortUrl) =>
        ShortUrlCacheKeyPrefix + shortUrl;
}

using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Lombiq.Marketing.UrlShortener.Events;
using Lombiq.Marketing.UrlShortener.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Marketing.UrlShortener.Middlewares;

public sealed class ShortUrlRedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<IShortUrlRedirectEventHandler> _redirectEventHandlers;

    public ShortUrlRedirectMiddleware(
        RequestDelegate next,
        IEnumerable<IShortUrlRedirectEventHandler> redirectEventHandlers)
    {
        _next = next;
        _redirectEventHandlers = redirectEventHandlers;
    }

    public async Task InvokeAsync(HttpContext context, IUrlShorteningService urlShorteningService)
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

        var destinationUrl = await urlShorteningService.GetDestinationUrlAsync(shortUrl);
        if (string.IsNullOrEmpty(destinationUrl))
        {
            await _next(context);
            return;
        }

        var redirectContext = new ShortUrlRedirectContext
        {
            HttpContext = context,
            ShortUrl = shortUrl,
            DestinationUrl = destinationUrl,
        };

        foreach (var redirectEventHandler in _redirectEventHandlers)
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
}

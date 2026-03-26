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

        var destinationUrl = await urlShorteningService.GetRedirectUrlAsync(shortUrl);
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

        foreach (var redirectEventHandler in redirectEventHandlers)
        {
            await redirectEventHandler.RedirectingAsync(redirectContext);
        }

        if (redirectContext.Cancel || string.IsNullOrEmpty(redirectContext.DestinationUrl))
        {
            await _next(context);
            return;
        }

        // We don't want this redirect to be cached by browsers, so we have full control over changes in the target URL.
        context.Response.Headers.CacheControl = "no-store, no-cache, max-age=0";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.Expires = "0";

        // This redirect comes from user input, but we assume that the user is the site owner or a user we trust.
        // It might be a good idea to add a whitelist of allowed domains for absolute URLs in the future.
#pragma warning disable SCS0027 // SCS0027: Potential Open Redirect vulnerability was found
        context.Response.Redirect(redirectContext.DestinationUrl, permanent: false);
#pragma warning restore SCS0027
    }
}

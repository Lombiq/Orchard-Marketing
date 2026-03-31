using Lombiq.Marketing.UrlShortener.Events;
using Lombiq.Marketing.UrlShortener.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Marketing.UrlShortener.Controllers;

[Route("jmp/")]
public class ShortUrlController : Controller
{
    private readonly IUrlShorteningService _urlShorteningService;
    private readonly IEnumerable<IShortUrlRedirectEventHandler> _redirectEventHandlers;
    private readonly ILogger<ShortUrlController> _logger;

    public ShortUrlController(
        IUrlShorteningService urlShorteningService,
        IEnumerable<IShortUrlRedirectEventHandler> redirectEventHandlers,
        ILogger<ShortUrlController> logger)
    {
        _urlShorteningService = urlShorteningService;
        _redirectEventHandlers = redirectEventHandlers;
        _logger = logger;
    }

    [Route("{shortUrl}")]
    public async Task<IActionResult> Index(string shortUrl)
    {
        var targetUrls = await _urlShorteningService.GetTargetUrlsAsync("/jmp/" + shortUrl);
        if (targetUrls is null)
        {
            return NotFound();
        }

        var redirectContext = new ShortUrlRedirectContext
        {
            HttpContext = HttpContext,
            ShortUrl = shortUrl,
            DestinationUrl = targetUrls.RedirectUrl,
            TrackingUrlWithUtmParameters = targetUrls.TrackingUrlWithUtmParameters,
        };

        try
        {
            foreach (var redirectEventHandler in _redirectEventHandlers)
            {
                await redirectEventHandler.RedirectingAsync(redirectContext);
            }

            if (redirectContext.Cancel)
            {
                return NotFound();
            }
        }
        catch (Exception e) when (!e.IsFatal())
        {
            _logger.LogError(e, "An error occurred while processing the short URL redirectEventHandlers for '{ShortUrl}'.", shortUrl);
        }

        // We don't want this redirect to be cached by browsers, so we have full control over changes in the target URL.
        HttpContext.Response.Headers.CacheControl = "no-store, no-cache, max-age=0";
        HttpContext.Response.Headers.Pragma = "no-cache";
        HttpContext.Response.Headers.Expires = "0";

        // This redirect comes from user input, but first we check if this is indeed a shortURL.
        // It might be a good idea to add an allow list of allowed domains for absolute URLs in the future.
#pragma warning disable SCS0027 // SCS0027: Potential Open Redirect vulnerability was found
        return Redirect(redirectContext.DestinationUrl);
#pragma warning restore SCS0027
    }
}

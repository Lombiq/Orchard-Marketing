using Lombiq.Marketing.Models;
using Lombiq.Marketing.UrlShortener.Events;
using Microsoft.AspNetCore.Http.Extensions;
using OrchardCore.Modules;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Services;

public sealed class MarketingShortUrlRedirectEventHandler : IShortUrlRedirectEventHandler
{
    private readonly IEnumerable<IShortUrlHitHandler> _shortUrlHitHandlers;
    private readonly IClock _clock;

    public MarketingShortUrlRedirectEventHandler(
        IEnumerable<IShortUrlHitHandler> shortUrlHitHandlers,
        IClock clock)
    {
        _shortUrlHitHandlers = shortUrlHitHandlers;
        _clock = clock;
    }

    public async Task RedirectingAsync(ShortUrlRedirectContext context)
    {
        var request = context.HttpContext.Request;
        var hitContext = new ShortUrlHitContext
        {
            ShortUrl = context.ShortUrl,
            DestinationUrl = context.DestinationUrl,
            Url = request.GetDisplayUrl(),
            Ip = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = request.Headers.UserAgent.ToString(),
            AcceptLanguage = request.Headers.AcceptLanguage.ToString(),
            Referrer = request.Headers.Referer.ToString(),
            SecChUa = request.Headers["Sec-CH-UA"].ToString(),
            SecChUaMobile = request.Headers["Sec-CH-UA-Mobile"].ToString(),
            SecChUaPlatform = request.Headers["Sec-CH-UA-Platform"].ToString(),
            SecChUaPlatformVersion = request.Headers["Sec-CH-UA-Platform-Version"].ToString(),
            SecChWidth = request.Headers["Sec-CH-Width"].ToString(),
            SecChViewportWidth = request.Headers["Sec-CH-Viewport-Width"].ToString(),
            DateTimeUtc = _clock.UtcNow,
            CancellationToken = context.HttpContext.RequestAborted,
        };

        foreach (var shortUrlHitHandler in _shortUrlHitHandlers)
        {
            await shortUrlHitHandler.HandleHitAsync(hitContext);
        }
    }
}

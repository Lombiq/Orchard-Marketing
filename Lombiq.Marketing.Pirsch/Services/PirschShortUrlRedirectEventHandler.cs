using Lombiq.Marketing.Pirsch.Models;
using Lombiq.Marketing.UrlShortener.Events;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Services;

public sealed class PirschShortUrlRedirectEventHandler : IShortUrlRedirectEventHandler
{
    private readonly ILogger<PirschShortUrlRedirectEventHandler> _logger;
    private readonly IPirschApiClient _pirschApiClient;
    private readonly IClock _clock;

    public PirschShortUrlRedirectEventHandler(
        ILogger<PirschShortUrlRedirectEventHandler> logger,
        IPirschApiClient pirschApiClient,
        IClock clock)
    {
        _logger = logger;
        _pirschApiClient = pirschApiClient;
        _clock = clock;
    }

    public async Task RedirectingAsync(ShortUrlRedirectContext context)
    {
        var request = context.HttpContext.Request;

        var success = await _pirschApiClient.SendHitAsync(
            new PirschHitRequest
            {
                Url = request.GetDisplayUrl(),
                Ip = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = request.Headers.UserAgent.ToString(),
                AcceptLanguage = request.Headers.AcceptLanguage.ToString(),
                SecChUa = request.Headers["Sec-CH-UA"].ToString(),
                SecChUaMobile = request.Headers["Sec-CH-UA-Mobile"].ToString(),
                SecChUaPlatform = request.Headers["Sec-CH-UA-Platform"].ToString(),
                SecChUaPlatformVersion = request.Headers["Sec-CH-UA-Platform-Version"].ToString(),
                SecChWidth = request.Headers["Sec-CH-Width"].ToString(),
                SecChViewportWidth = request.Headers["Sec-CH-Viewport-Width"].ToString(),
                Referrer = request.Headers.Referer.ToString(),
                Time = _clock.UtcNow,
            },
            context.HttpContext.RequestAborted);

        if (!success)
        {
            _logger.LogError(
                "Failed to send a hit to Pirsch API for the redirected short URL: {Url}. Check logs for more info",
                request.GetDisplayUrl());
        }
        else
        {
            _logger.LogInformation(
                "Successfully sent a hit to Pirsch API for the redirected short URL: {Url}",
                request.GetDisplayUrl());
        }
    }
}

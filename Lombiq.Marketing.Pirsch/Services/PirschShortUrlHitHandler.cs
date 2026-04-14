using Lombiq.Marketing.Models;
using Lombiq.Marketing.Pirsch.Models;
using Lombiq.Marketing.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Services;

public sealed class PirschShortUrlHitHandler : IShortUrlHitHandler
{
    private readonly ILogger<PirschShortUrlHitHandler> _logger;
    private readonly IPirschApiClient _pirschApiClient;

    public PirschShortUrlHitHandler(
        ILogger<PirschShortUrlHitHandler> logger,
        IPirschApiClient pirschApiClient)
    {
        _logger = logger;
        _pirschApiClient = pirschApiClient;
    }

    public async Task HandleHitAsync(ShortUrlHitContext context)
    {
        var response = await _pirschApiClient.SendHitResponseAsync(
            new PirschHitRequest
            {
                Url = context.TrackingUrlWithUtmParameters,
                Ip = context.Ip,
                UserAgent = context.UserAgent,
                AcceptLanguage = context.AcceptLanguage,
                SecChUa = context.SecChUa,
                SecChUaMobile = context.SecChUaMobile,
                SecChUaPlatform = context.SecChUaPlatform,
                SecChUaPlatformVersion = context.SecChUaPlatformVersion,
                SecChWidth = context.SecChWidth,
                SecChViewportWidth = context.SecChViewportWidth,
                Referrer = context.Referrer,
                Time = context.DateTimeUtc,
            },
            context.CancellationToken);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(
                "Successfully sent a hit to Pirsch API for the tracked target URL: {Url}.",
                context.TrackingUrlWithUtmParameters);
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync(context.CancellationToken);
            _logger.LogError(
                "Failed to send a hit to Pirsch API for the tracked target URL: {Url}. Response: {ResponseContent}",
                context.TrackingUrlWithUtmParameters,
                responseContent);
        }
    }
}

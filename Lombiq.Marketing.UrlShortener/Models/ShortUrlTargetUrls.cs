namespace Lombiq.Marketing.UrlShortener.Models;

public sealed class ShortUrlTargetUrls
{
    public required string RedirectUrl { get; init; }

    public required string TrackingUrlWithUtmParameters { get; init; }
}

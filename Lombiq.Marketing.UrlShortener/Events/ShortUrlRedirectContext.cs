using Microsoft.AspNetCore.Http;

namespace Lombiq.Marketing.UrlShortener.Events;

public sealed class ShortUrlRedirectContext
{
    public required HttpContext HttpContext { get; init; }

    public required string ShortUrl { get; init; }

    public required string DestinationUrl { get; set; }

    public bool Cancel { get; set; }
}

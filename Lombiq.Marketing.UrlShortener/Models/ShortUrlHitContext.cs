using System;
using System.Threading;

namespace Lombiq.Marketing.UrlShortener.Models;

public sealed class ShortUrlHitContext
{
    public required string ShortUrl { get; init; }

    public required string DestinationUrl { get; init; }

    public required string TrackingUrlWithUtmParameters { get; init; }

    public string? Ip { get; init; }

    public string? UserAgent { get; init; }

    public string? AcceptLanguage { get; init; }

    public string? Referrer { get; init; }

    public string? SecChUa { get; init; }

    public string? SecChUaMobile { get; init; }

    public string? SecChUaPlatform { get; init; }

    public string? SecChUaPlatformVersion { get; init; }

    public string? SecChWidth { get; init; }

    public string? SecChViewportWidth { get; init; }

    public DateTime DateTimeUtc { get; init; }

    public CancellationToken CancellationToken { get; init; }
}

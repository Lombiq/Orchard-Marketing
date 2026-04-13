using Lombiq.Marketing.UrlShortener.Models;
using Lombiq.Marketing.UrlShortener.Services;
using System.Threading.Tasks;

namespace Lombiq.Marketing.UrlShortener.Extensions;

/// <summary>
/// Provides convenience overloads for working with <see cref="IUrlShorteningService"/>.
/// </summary>
public static class UrlShorteningServiceExtensions
{
    /// <summary>
    /// Checks whether the short URL of the supplied content part is unique among published short URL content items.
    /// </summary>
    /// <param name="urlShorteningService">The URL shortening service.</param>
    /// <param name="shortUrlPart">The short URL part to validate.</param>
    /// <returns><see langword="true"/> if the short URL is unique. Otherwise <see langword="false"/>.</returns>
    public static Task<bool> IsShortUrlUniqueAsync(this IUrlShorteningService urlShorteningService, ShortUrlPart shortUrlPart) =>
        urlShorteningService.IsShortUrlUniqueAsync(shortUrlPart.ShortUrl.Text, shortUrlPart.ContentItem.ContentItemId);
}

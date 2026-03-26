using Lombiq.Marketing.UrlShortener.Models;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace Lombiq.Marketing.UrlShortener.Services;

/// <summary>
/// Provides CRUD-style operations for persisted short URL mappings.
/// </summary>
public interface IUrlShorteningService
{
    /// <summary>
    /// Gets the final redirect URL for a short URL.
    /// </summary>
    /// <param name="shortUrl">The short URL path.</param>
    /// <returns>The redirect URL, or <see langword="null"/> if no mapping exists.</returns>
    Task<string?> GetRedirectUrlAsync(string shortUrl);

    /// <summary>
    /// Checks whether the short URL value of the supplied part is unique.
    /// </summary>
    /// <param name="shortUrlPart">The short URL part to validate.</param>
    /// <returns><see langword="true"/> if the short URL is unique. Otherwise <see langword="false"/>.</returns>
    Task<bool> IsShortUrlUniqueAsync(ShortUrlPart shortUrlPart);

    /// <summary>
    /// Updates an existing short URL mapping.
    /// </summary>
    /// <param name="previousShortUrl">The previous short URL value.</param>
    /// <param name="shortUrlPart">The updated short URL part.</param>
    /// <returns><see langword="true"/> if the mapping was updated. Otherwise <see langword="false"/>.</returns>
    Task<bool> UpdateShortUrlAsync(string previousShortUrl, ShortUrlPart shortUrlPart);

    /// <summary>
    /// Deletes the short URL mapping for the supplied content item.
    /// </summary>
    /// <param name="shortUrlContentItem">The content item to delete the mapping for.</param>
    Task DeleteShortUrlAsync(ContentItem shortUrlContentItem);
}

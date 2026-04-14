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
    /// Gets the redirect and tracking URLs for a short URL.
    /// </summary>
    /// <param name="shortUrl">The short URL path.</param>
    /// <returns>The target URLs, or <see langword="null"/> if no mapping exists.</returns>
    Task<ShortUrlTargetUrls?> GetTargetUrlsAsync(string shortUrl);

    /// <summary>
    /// Checks whether a short URL is unique among published short URL content items.
    /// </summary>
    /// <param name="shortUrl">The short URL path to check.</param>
    /// <param name="contentItemId">The optional content item ID to exclude from the uniqueness check, useful for
    /// current content item save.</param>
    /// <returns><see langword="true"/> if the short URL is unique. Otherwise <see langword="false"/>.</returns>
    Task<bool> IsShortUrlUniqueAsync(string shortUrl, string? contentItemId = null);

    /// <summary>
    /// Updates an existing short URL mapping.
    /// </summary>
    /// <param name="previousShortUrl">The previous short URL value.</param>
    /// <param name="shortUrlPart">The updated short URL part.</param>
    /// <returns><see langword="true"/> if the mapping was updated. Otherwise <see langword="false"/>.</returns>
    Task<bool> UpdateShortUrlAsync(string? previousShortUrl, ShortUrlPart shortUrlPart);

    /// <summary>
    /// Deletes the short URL mapping for the supplied content item.
    /// </summary>
    /// <param name="shortUrlContentItem">The content item to delete the mapping for.</param>
    Task DeleteShortUrlAsync(ContentItem shortUrlContentItem);
}

using Lombiq.Marketing.UrlShortener.Indexes;
using Lombiq.Marketing.UrlShortener.Models;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Marketing.UrlShortener.Services;

public class UrlShorteningService : IUrlShorteningService
{
    private const string ShortUrlCacheKeyPrefix = "ShortUrlRedirectInfo:";

    private readonly ISession _session;
    private readonly IMemoryCache _memoryCache;

    public UrlShorteningService(ISession session, IMemoryCache memoryCache)
    {
        _session = session;
        _memoryCache = memoryCache;
    }

    public async Task<ShortUrlRedirectInfo?> GetRedirectInfoAsync(string shortUrl)
    {
        if (_memoryCache.TryGetValue(GetCacheKey(shortUrl), out ShortUrlRedirectInfo redirectInfo))
        {
            return redirectInfo;
        }

        ShortUrlRedirectInfo? cachedRedirectInfo = null;

        if (await _session.Query<ContentItem, ShortUrlPartIndex>(index => index.ShortUrl == shortUrl)
            .FirstOrDefaultAsync() is { } shortUrlContentItem)
        {
            cachedRedirectInfo = CreateRedirectInfo(shortUrlContentItem);
            _memoryCache.Set(GetCacheKey(shortUrl), cachedRedirectInfo);
        }

        return cachedRedirectInfo;
    }

    public async Task<bool> IsShortUrlUniqueAsync(ShortUrlPart shortUrlPart)
    {
        // Check if the short URL already exists in the cache. This is a quick check to avoid hitting the database if
        // we already know the short URL is taken.
        if (_memoryCache.TryGetValue(GetCacheKey(shortUrlPart.ShortUrl.Text), out ShortUrlRedirectInfo _))
        {
            return false;
        }

        // Check if the short URL already exists in the database.
        if (await _session.Query<ContentItem, ShortUrlPartIndex>(index =>
                index.ShortUrl == shortUrlPart.ShortUrl.Text &&
                index.ContentItemId != shortUrlPart.ContentItem.ContentItemId)
            .FirstOrDefaultAsync() is { } existingShortUrl)
        {
            // Cache the existing short URL to prevent future database hits for the same short URL.
            _memoryCache.Set(GetCacheKey(shortUrlPart.ShortUrl.Text), CreateRedirectInfo(existingShortUrl));
            return false;
        }

        return true;
    }

    public async Task<bool> UpdateShortUrlAsync(string previousShortUrl, ShortUrlPart shortUrlPart)
    {
        // If the short URL is being changed, we need to check if the new short URL is unique.
        if (!string.IsNullOrEmpty(previousShortUrl) &&
            shortUrlPart.ShortUrl.Text != previousShortUrl &&
            !await IsShortUrlUniqueAsync(shortUrlPart))
        {
            return false;
        }

        _memoryCache.Set(GetCacheKey(shortUrlPart.ShortUrl.Text), CreateRedirectInfo(shortUrlPart.ContentItem));

        if (!string.IsNullOrEmpty(previousShortUrl) && shortUrlPart.ShortUrl.Text != previousShortUrl)
        {
            _memoryCache.Remove(GetCacheKey(previousShortUrl));
        }

        return true;
    }

    public Task DeleteShortUrlAsync(ContentItem shortUrlContentItem)
    {
        // Remove the short URL from the cache to ensure it doesn't return stale data after deletion.
        _memoryCache.Remove(GetCacheKey(shortUrlContentItem.As<ShortUrlPart>().ShortUrl.Text));
        return Task.CompletedTask;
    }

    private static ShortUrlRedirectInfo CreateRedirectInfo(ContentItem shortUrlContentItem)
    {
        var shortUrlPart = shortUrlContentItem.As<ShortUrlPart>();
        var utmPart = shortUrlContentItem.As<UtmPart>();

        return new ShortUrlRedirectInfo
        {
            DestinationUrl = shortUrlPart.DestinationUrl.Text,
            UtmSource = utmPart?.UtmSource.Text,
            UtmMedium = utmPart?.UtmMedium.Text,
            UtmCampaign = utmPart?.UtmCampaign.Text,
            UtmContent = utmPart?.UtmContent.Text,
            UtmTerm = utmPart?.Term.Text,
        };
    }

    private string GetCacheKey(string shortUrl) =>
        ShortUrlCacheKeyPrefix + shortUrl;
}

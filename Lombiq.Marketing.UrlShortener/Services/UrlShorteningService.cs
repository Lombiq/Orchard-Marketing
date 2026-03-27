using Lombiq.Marketing.UrlShortener.Indexes;
using Lombiq.Marketing.UrlShortener.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Marketing.UrlShortener.Services;

public class UrlShorteningService : IUrlShorteningService
{
    private const string ShortUrlCacheKeyPrefix = "ShortUrlTargetUrls:";

    private readonly ISession _session;
    private readonly IMemoryCache _memoryCache;

    public UrlShorteningService(ISession session, IMemoryCache memoryCache)
    {
        _session = session;
        _memoryCache = memoryCache;
    }

    public async Task<ShortUrlTargetUrls?> GetTargetUrlsAsync(string shortUrl)
    {
        if (_memoryCache.TryGetValue(GetCacheKey(shortUrl), out ShortUrlTargetUrls? targetUrls))
        {
            return targetUrls;
        }

        ShortUrlTargetUrls? cachedTargetUrls = null;

        if (await _session.Query<ContentItem, ContentItemIndex>(index => index.Published)
            .With<ShortUrlPartIndex>(index => index.ShortUrl == shortUrl)
            .FirstOrDefaultAsync() is { } shortUrlContentItem)
        {
            cachedTargetUrls = SetCache(shortUrlContentItem);
        }

        return cachedTargetUrls;
    }

    public async Task<bool> IsShortUrlUniqueAsync(ShortUrlPart shortUrlPart)
    {
        if (_memoryCache.TryGetValue(GetCacheKey(shortUrlPart.ShortUrl.Text), out ShortUrlTargetUrls? _))
        {
            return false;
        }

        if (await _session.Query<ContentItem, ContentItemIndex>(index => index.Published)
            .With<ShortUrlPartIndex>(index =>
                index.ShortUrl == shortUrlPart.ShortUrl.Text &&
                index.ContentItemId != shortUrlPart.ContentItem.ContentItemId)
            .FirstOrDefaultAsync() is { } existingShortUrl)
        {
            SetCache(existingShortUrl);
            return false;
        }

        return true;
    }

    public async Task<bool> UpdateShortUrlAsync(string? previousShortUrl, ShortUrlPart shortUrlPart)
    {
        if (!string.IsNullOrEmpty(previousShortUrl) &&
            shortUrlPart.ShortUrl.Text != previousShortUrl &&
            !await IsShortUrlUniqueAsync(shortUrlPart))
        {
            return false;
        }

        SetCache(shortUrlPart);

        if (!string.IsNullOrEmpty(previousShortUrl) && shortUrlPart.ShortUrl.Text != previousShortUrl)
        {
            RemoveCache(previousShortUrl);
        }

        return true;
    }

    public Task DeleteShortUrlAsync(ContentItem shortUrlContentItem)
    {
        RemoveCache(shortUrlContentItem.As<ShortUrlPart>().ShortUrl.Text);
        return Task.CompletedTask;
    }

    private ShortUrlTargetUrls SetCache(ContentItem shortUrlContentItem) =>
        SetCache(shortUrlContentItem.As<ShortUrlPart>());

    private ShortUrlTargetUrls SetCache(ShortUrlPart shortUrlPart)
    {
        var targetUrls = BuildTargetUrls(shortUrlPart.ContentItem);
        _memoryCache.Set(GetCacheKey(shortUrlPart.ShortUrl.Text), targetUrls);

        return targetUrls;
    }

    private void RemoveCache(string shortUrl) =>
        _memoryCache.Remove(GetCacheKey(shortUrl));

    private static ShortUrlTargetUrls BuildTargetUrls(ContentItem shortUrlContentItem)
    {
        var shortUrlPart = shortUrlContentItem.As<ShortUrlPart>();
        var utmPart = shortUrlContentItem.As<UtmPart>();

        return new ShortUrlTargetUrls
        {
            RedirectUrl = shortUrlPart.DestinationUrl.Text,
            TrackingUrlWithUtmParameters = BuildTrackingUrl(shortUrlPart.DestinationUrl.Text, utmPart),
        };
    }

    private static string BuildTrackingUrl(string destinationUrl, UtmPart utmPart)
    {
        if (Uri.TryCreate(destinationUrl, UriKind.RelativeOrAbsolute, out var destinationUri) && destinationUri.IsAbsoluteUri)
        {
            return BuildAbsoluteTrackingUrl(destinationUri, utmPart);
        }

        return BuildRelativeTrackingUrl(destinationUrl, utmPart);
    }

    private static string BuildAbsoluteTrackingUrl(Uri destinationUri, UtmPart utmPart)
    {
        var queryParameters = CreateQueryParameters(destinationUri.Query, utmPart);
        var queryBuilder = BuildQueryString(queryParameters);

        var uriBuilder = new UriBuilder(destinationUri)
        {
            Query = queryBuilder.ToString().TrimStart('?'),
        };

        return uriBuilder.Uri.ToString();
    }

    private static string BuildRelativeTrackingUrl(string destinationUrl, UtmPart utmPart)
    {
        var fragmentStartIndex = destinationUrl.IndexOf('#');
        var fragment = fragmentStartIndex >= 0 ? destinationUrl[fragmentStartIndex..] : string.Empty;
        var destinationUrlWithoutFragment = fragmentStartIndex >= 0
            ? destinationUrl[..fragmentStartIndex]
            : destinationUrl;

        var queryStartIndex = destinationUrlWithoutFragment.IndexOf('?');
        var path = queryStartIndex >= 0
            ? destinationUrlWithoutFragment[..queryStartIndex]
            : destinationUrlWithoutFragment;
        var queryString = queryStartIndex >= 0
            ? destinationUrlWithoutFragment[queryStartIndex..]
            : string.Empty;

        var queryBuilder = BuildQueryString(CreateQueryParameters(queryString, utmPart));

        return $"{path}{queryBuilder}{fragment}";
    }

    private static Dictionary<string, StringValues> CreateQueryParameters(string queryString, UtmPart utmPart)
    {
        var queryParameters = new Dictionary<string, StringValues>(QueryHelpers.ParseQuery(queryString), StringComparer.OrdinalIgnoreCase);

        SetQueryParameter(queryParameters, "utm_source", utmPart.UtmSource.Text);
        SetQueryParameter(queryParameters, "utm_medium", utmPart.UtmMedium.Text);
        SetQueryParameter(queryParameters, "utm_campaign", utmPart.UtmCampaign.Text);
        SetQueryParameter(queryParameters, "utm_content", utmPart.UtmContent.Text);
        SetQueryParameter(queryParameters, "utm_term", utmPart.UtmTerm.Text);

        return queryParameters;
    }

    private static QueryBuilder BuildQueryString(IDictionary<string, StringValues> queryParameters)
    {
        var queryBuilder = new QueryBuilder();
        foreach (var parameter in queryParameters)
        {
            foreach (var value in parameter.Value.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                queryBuilder.Add(parameter.Key, value!);
            }
        }

        return queryBuilder;
    }

    private static void SetQueryParameter(Dictionary<string, StringValues> queryParameters, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            queryParameters.Remove(key);
            return;
        }

        queryParameters[key] = value;
    }

    private static string GetCacheKey(string shortUrl) =>
        ShortUrlCacheKeyPrefix + shortUrl;
}

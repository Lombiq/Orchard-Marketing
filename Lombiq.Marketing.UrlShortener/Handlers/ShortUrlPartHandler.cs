using Lombiq.Marketing.UrlShortener.Indexes;
using Lombiq.Marketing.UrlShortener.Models;
using Lombiq.Marketing.UrlShortener.Services;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using System;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Marketing.UrlShortener.Handlers;

public class ShortUrlPartHandler : ContentPartHandler<ShortUrlPart>
{
    private readonly IUrlShorteningService _urlShorteningService;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly ISession _session;
    private readonly IMemoryCache _memoryCache;
    private readonly IClock _clock;

    private ShortUrlPart _previousShortUrlPart;

    public ShortUrlPartHandler(
        IUrlShorteningService urlShorteningService,
        IUpdateModelAccessor updateModelAccessor,
        IMemoryCache memoryCache,
        ISession session,
        IClock clock)
    {
        _urlShorteningService = urlShorteningService;
        _updateModelAccessor = updateModelAccessor;
        _memoryCache = memoryCache;
        _session = session;
        _clock = clock;
    }

    public override async Task InitializingAsync(InitializingContentContext context, ShortUrlPart part)
    {
        part.ShortUrl.Text = await GenerateRandomShortUrlAsync();
        part.ContentItem.Apply(part);
    }

    public override Task UpdatingAsync(UpdateContentContext context, ShortUrlPart part)
    {
        _previousShortUrlPart = part;
        return Task.CompletedTask;
    }

    public override Task CreatedAsync(CreateContentContext context, ShortUrlPart part) => UpdateShortUrlAsync(part);

    public override Task UpdatedAsync(UpdateContentContext context, ShortUrlPart part) => UpdateShortUrlAsync(part);

    public override Task RemovedAsync(RemoveContentContext context, ShortUrlPart part) => _urlShorteningService.DeleteShortUrlAsync(part.ContentItem);

    private async Task UpdateShortUrlAsync(ShortUrlPart part)
    {
        if (string.IsNullOrEmpty(part.ShortUrl.Text))
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                nameof(ShortUrlPart.ShortUrl),
                "The short URL is required.");
        }

        if (string.IsNullOrEmpty(part.DestinationUrl.Text))
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                nameof(ShortUrlPart.DestinationUrl),
                "The destination URL is required.");
        }

        if (!Uri.IsWellFormedUriString(part.ShortUrl.Text, UriKind.Relative) || !part.ShortUrl.Text.StartsWith('/'))
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                nameof(ShortUrlPart.ShortUrl),
                "The short URL must be a valid relative URL (for example: /short-url).");
        }

        if (!Uri.TryCreate(part.DestinationUrl.Text, UriKind.RelativeOrAbsolute, out var destinationUri))
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                nameof(ShortUrlPart.DestinationUrl),
                "The destination URL must be a valid URL.");
        }

        if (destinationUri != null && !destinationUri.IsAbsoluteUri && !destinationUri.OriginalString.StartsWith('/'))
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                nameof(ShortUrlPart.DestinationUrl),
                "The destination URL must be an absolute URL or a relative URL starting with '/'.");
        }

        if (!_updateModelAccessor.ModelUpdater.ModelState.IsValid)
        {
            return;
        }

        if (!await _urlShorteningService.UpdateShortUrlAsync(_previousShortUrlPart.ShortUrl.Text, part))
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                nameof(ShortUrlPart.ShortUrl),
                "The short URL must be unique. The provided short URL is already in use.");
        }

        _previousShortUrlPart = part;
    }

    private async Task<string> GenerateRandomShortUrlAsync()
    {
        var isUnique = false;
        var randomShortUrl = string.Empty;
        while (!isUnique)
        {
            var sourceString = $"{_clock.UtcNow.Ticks.ToTechnicalString()}_{Guid.NewGuid()}";

            randomShortUrl = $"{sourceString.GetHashCode(StringComparison.OrdinalIgnoreCase):X}";

            if (!_memoryCache.TryGetValue($"/{randomShortUrl}", out _))
            {
                var url = randomShortUrl;
                isUnique = (await _session.QueryIndex<ShortUrlPartIndex>(index => index.ShortUrl == $"/{url}")
                    .FirstOrDefaultAsync()) == null;
            }
        }

        return $"/{randomShortUrl}";
    }
}

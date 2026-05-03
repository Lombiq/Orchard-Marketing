using Lombiq.Marketing.UrlShortener.Controllers;
using Lombiq.Marketing.UrlShortener.Models;
using Lombiq.Marketing.UrlShortener.Services;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using System;
using System.Threading.Tasks;
using StringExtensions = OrchardCore.Modules.StringExtensions;

namespace Lombiq.Marketing.UrlShortener.Handlers;

public class ShortUrlPartHandler : ContentPartHandler<ShortUrlPart>
{
    private readonly IUrlShorteningService _urlShorteningService;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IClock _clock;

    private ShortUrlPart? _previousShortUrlPart;

    public ShortUrlPartHandler(
        IUrlShorteningService urlShorteningService,
        IUpdateModelAccessor updateModelAccessor,
        IClock clock)
    {
        _urlShorteningService = urlShorteningService;
        _updateModelAccessor = updateModelAccessor;
        _clock = clock;
    }

    public override async Task InitializingAsync(InitializingContentContext context, ShortUrlPart part)
    {
        part.ShortUrl.Text = await GenerateRandomShortUrlAsync();
        part.ContentItem.Apply(part);
    }

    public override Task CreatedAsync(CreateContentContext context, ShortUrlPart part) => UpdateShortUrlAsync(part);

    public override async Task ClonedAsync(CloneContentContext context, ShortUrlPart part)
    {
        var clonedPart = context.CloneContentItem.As<ShortUrlPart>();
        clonedPart.ShortUrl.Text = await GenerateRandomShortUrlAsync();
        context.CloneContentItem.Apply(clonedPart);
    }

    public override Task UpdatingAsync(UpdateContentContext context, ShortUrlPart part)
    {
        _previousShortUrlPart = part;
        return Task.CompletedTask;
    }

    public override Task UpdatedAsync(UpdateContentContext context, ShortUrlPart part) => UpdateShortUrlAsync(part);

    public override Task RemovedAsync(RemoveContentContext context, ShortUrlPart part) => _urlShorteningService.DeleteShortUrlAsync(part.ContentItem);

    public override Task GetContentItemAspectAsync(ContentItemAspectContext context, ShortUrlPart part) =>
        context.ForAsync<ContentItemMetadata>(contentItemMetadata =>
        {
            contentItemMetadata.DisplayRouteValues = new RouteValueDictionary
            {
                { "Area", "Lombiq.Marketing.UrlShortener" },
                { "Controller", typeof(ShortUrlController).ControllerName() },
                { "Action", nameof(ShortUrlController.Index) },
                { "ShortUrl", part.ShortUrl.Text["/jmp/".Length..] },
            };

            return Task.CompletedTask;
        });

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

        if (!Uri.IsWellFormedUriString(part.ShortUrl.Text, UriKind.Relative))
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                nameof(ShortUrlPart.ShortUrl),
                "The short URL must be a valid relative URL (for example: /jmp/short-url).");
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

        if (!StringExtensions.StartsWithOrdinalIgnoreCase(part.ShortUrl.Text, "/jmp"))
        {
            part.ShortUrl.Text = "/jmp/" + part.ShortUrl.Text.TrimStart('/');
            part.Apply();
        }

        if (!await _urlShorteningService.UpdateShortUrlAsync(_previousShortUrlPart?.ShortUrl.Text, part))
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                nameof(ShortUrlPart.ShortUrl),
                "The short URL must be unique. The provided short URL is already in use.");
        }

        if (string.IsNullOrEmpty(part.ContentItem.DisplayText))
        {
            part.ContentItem.DisplayText = part.DestinationUrl.Text;
        }

        _previousShortUrlPart = part;
    }

    private async Task<string> GenerateRandomShortUrlAsync()
    {
        var isUnique = false;
        var shortUrlWithPrefix = string.Empty;
        while (!isUnique)
        {
            var sourceString = $"{_clock.UtcNow.Ticks.ToTechnicalString()}_{Guid.NewGuid()}";

            var randomShortUrl = $"{sourceString.GetHashCode(StringComparison.OrdinalIgnoreCase):X}";
            shortUrlWithPrefix = $"/jmp/{randomShortUrl}";

            isUnique = await _urlShorteningService.IsShortUrlUniqueAsync(shortUrlWithPrefix);
        }

        return shortUrlWithPrefix;
    }
}

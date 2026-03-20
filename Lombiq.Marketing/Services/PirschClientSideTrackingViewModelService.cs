using AngleSharp.Html.Parser;
using Lombiq.Marketing.Models;
using Lombiq.Marketing.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Cache;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Services;

public sealed class PirschClientSideTrackingViewModelService : IPirschClientSideTrackingViewModelService
{
    private const string MemoryCacheKeyPrefix = "Lombiq.Marketing.PirschClientSideTracking";
    private const string CacheKey = $"{MemoryCacheKeyPrefix}:{nameof(PirschClientSideTrackingViewModel)}";

    private readonly IHostEnvironment _environment;
    private readonly IMemoryCache _memoryCache;
    private readonly IOptionsSnapshot<PirschSettings> _pirschSettingsOptions;
    private readonly ISignal _signal;

    public PirschClientSideTrackingViewModelService(
        IHostEnvironment environment,
        IMemoryCache memoryCache,
        IOptionsSnapshot<PirschSettings> pirschSettingsOptions,
        ISignal signal)
    {
        _environment = environment;
        _memoryCache = memoryCache;
        _pirschSettingsOptions = pirschSettingsOptions;
        _signal = signal;
    }

    public async Task<PirschClientSideTrackingViewModel> GetViewModelAsync()
    {
        if (_memoryCache.TryGetValue(CacheKey, out PirschClientSideTrackingViewModel viewModel))
        {
            return viewModel;
        }

        var snippetHtml = _pirschSettingsOptions.Value.ClientSideCodeSnippet;
        var renderedSnippet = snippetHtml;

        if (_environment.IsDevelopment() && !string.IsNullOrWhiteSpace(snippetHtml))
        {
            var parser = new HtmlParser();
            var script = (await parser.ParseDocumentAsync(snippetHtml)).Scripts.FirstOrDefault();
            if (script != null && !script.HasAttribute("data-dev"))
            {
                script.SetAttribute("data-dev", string.Empty);
                renderedSnippet = script.OuterHtml;
            }
        }

        viewModel = new PirschClientSideTrackingViewModel
        {
            ClientSideCodeSnippet = renderedSnippet ?? string.Empty,
        };

        _memoryCache.Set(CacheKey, viewModel, _signal.GetToken(MemoryCacheKeyPrefix));

        return viewModel;
    }

    public Task InvalidateCachedViewModelAsync() => _signal.SignalTokenAsync(MemoryCacheKeyPrefix);
}

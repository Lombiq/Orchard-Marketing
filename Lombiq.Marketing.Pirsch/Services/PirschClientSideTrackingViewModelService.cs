using AngleSharp.Html.Parser;
using Lombiq.Marketing.Pirsch.Models;
using Lombiq.Marketing.Pirsch.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Cache;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Services;

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

    public async Task<PirschClientSideTrackingViewModel?> GetViewModelAsync()
    {
        if (_memoryCache.TryGetValue(CacheKey, out PirschClientSideTrackingViewModel? viewModel))
        {
            return viewModel;
        }

        var settings = _pirschSettingsOptions.Value;
        var snippetHtml = settings.ClientSideCodeSnippet;
        var renderedSnippet = snippetHtml;

        if (_environment.IsDevelopment() && !string.IsNullOrWhiteSpace(snippetHtml))
        {
            var parser = new HtmlParser();
            var script = (await parser.ParseDocumentAsync(snippetHtml)).Scripts.FirstOrDefault();

            if (script != null)
            {
                var fallbackDataDev = settings.DataDev;

                if (!string.IsNullOrEmpty(script.GetAttribute("data-dev")))
                {
                    renderedSnippet = PirschSettingsSanitizer.SerializeScript(script);
                }
                else if (string.IsNullOrEmpty(script.GetAttribute("data-dev")) &&
                    !string.IsNullOrWhiteSpace(fallbackDataDev))
                {
                    script.SetAttribute("data-dev", fallbackDataDev);
                    renderedSnippet = PirschSettingsSanitizer.SerializeScript(script);
                }
            }
        }

        viewModel = new PirschClientSideTrackingViewModel
        {
            ClientSideCodeSnippet = renderedSnippet,
        };

        _memoryCache.Set(CacheKey, viewModel, _signal.GetToken(MemoryCacheKeyPrefix));

        return viewModel;
    }

    public Task InvalidateCachedViewModelAsync() => _signal.SignalTokenAsync(MemoryCacheKeyPrefix);
}

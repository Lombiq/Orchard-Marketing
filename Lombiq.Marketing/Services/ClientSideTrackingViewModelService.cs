using Lombiq.Marketing.Models;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Services;

public sealed class ClientSideTrackingViewModelService : IClientSideTrackingViewModelService
{
    private const string MemoryCacheKeyPrefix = "Lombiq.Marketing.ClientSideTracking";
    private const string CacheKey = $"{MemoryCacheKeyPrefix}:{nameof(ClientSideTrackingViewModel)}";

    private readonly IMemoryCache _memoryCache;
    private readonly IEnumerable<IClientSideTrackingProvider> _providers;
    private readonly ISignal _signal;

    public ClientSideTrackingViewModelService(
        IMemoryCache memoryCache,
        IEnumerable<IClientSideTrackingProvider> providers,
        ISignal signal)
    {
        _memoryCache = memoryCache;
        _providers = providers;
        _signal = signal;
    }

    public async Task<ClientSideTrackingViewModel?> GetViewModelAsync()
    {
        if (_memoryCache.TryGetValue(CacheKey, out ClientSideTrackingViewModel? viewModel))
        {
            return viewModel;
        }

        var markups = new List<string>();
        foreach (var provider in _providers)
        {
            var markup = await provider.GetClientSideTrackingMarkupAsync();
            if (!string.IsNullOrWhiteSpace(markup)) markups.Add(markup);
        }

        viewModel = new ClientSideTrackingViewModel
        {
            Html = string.Join(Environment.NewLine, markups.Where(markup => !string.IsNullOrWhiteSpace(markup))),
        };

        _memoryCache.Set(CacheKey, viewModel, _signal.GetToken(MemoryCacheKeyPrefix));

        return viewModel;
    }

    public Task InvalidateCachedViewModelAsync() => _signal.SignalTokenAsync(MemoryCacheKeyPrefix);
}

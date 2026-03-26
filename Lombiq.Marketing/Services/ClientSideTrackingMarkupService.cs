using Lombiq.Marketing.Models;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Cache;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Services;

public sealed class ClientSideTrackingMarkupService : IClientSideTrackingMarkupService
{
    private const string MemoryCacheKeyPrefix = "Lombiq.Marketing.ClientSideTracking";
    private const string CacheKey = $"{MemoryCacheKeyPrefix}:{nameof(GetViewModelsAsync)}";

    private readonly IMemoryCache _memoryCache;
    private readonly IEnumerable<IClientSideTrackingProvider> _providers;
    private readonly ISignal _signal;

    public ClientSideTrackingMarkupService(
        IMemoryCache memoryCache,
        IEnumerable<IClientSideTrackingProvider> providers,
        ISignal signal)
    {
        _memoryCache = memoryCache;
        _providers = providers;
        _signal = signal;
    }

    public async Task<IReadOnlyList<ClientSideTrackingViewModel>> GetViewModelsAsync()
    {
        if (_memoryCache.TryGetValue(CacheKey, out IReadOnlyList<ClientSideTrackingViewModel>? viewModels))
        {
            return viewModels;
        }

        var builtViewModels = new List<ClientSideTrackingViewModel>();

        foreach (var provider in _providers)
        {
            var markup = await provider.GetClientSideTrackingMarkupAsync();
            if (string.IsNullOrWhiteSpace(markup))
            {
                continue;
            }

            builtViewModels.Add(new ClientSideTrackingViewModel
            {
                Html = markup,
                Zone = await provider.GetClientSideTrackingZoneAsync() ?? string.Empty,
            });
        }

        viewModels = builtViewModels;

        _memoryCache.Set(CacheKey, viewModels, _signal.GetToken(MemoryCacheKeyPrefix));

        return viewModels;
    }

    public Task InvalidateCachedViewModelAsync() => _signal.SignalTokenAsync(MemoryCacheKeyPrefix);
}

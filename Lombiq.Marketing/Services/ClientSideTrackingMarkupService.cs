using Lombiq.Marketing.Models;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Cache;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Services;

public sealed class ClientSideTrackingMarkupService : IClientSideTrackingMarkupService
{
    private const string CacheKey = "Lombiq.Marketing.ClientSideTracking";

    private readonly IEnumerable<IClientSideTrackingProvider> _providers;
    private readonly IMemoryCache _memoryCache;
    private readonly ISignal _signal;

    public ClientSideTrackingMarkupService(
        IEnumerable<IClientSideTrackingProvider> providers,
        IMemoryCache memoryCache,
        ISignal signal)
    {
        _providers = providers;
        _memoryCache = memoryCache;
        _signal = signal;
    }

    public async Task<IReadOnlyList<ClientSideTrackingViewModel>?> GetViewModelsAsync()
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
                Zone = await provider.GetClientSideTrackingZoneAsync(),
            });
        }

        viewModels = builtViewModels;

        _memoryCache.Set(CacheKey, viewModels, _signal.GetToken(CacheKey));

        return viewModels;
    }

    public Task InvalidateCachedViewModelAsync() => _signal.SignalTokenAsync(CacheKey);
}

using Lombiq.Marketing.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Services;

public sealed class ClientSideTrackingMarkupService : IClientSideTrackingMarkupService
{
    private const string DistributedCacheKey = "Lombiq.Marketing.ClientSideTracking";

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _distributedCache;
    private readonly IEnumerable<IClientSideTrackingProvider> _providers;

    public ClientSideTrackingMarkupService(
        IDistributedCache distributedCache,
        IEnumerable<IClientSideTrackingProvider> providers)
    {
        _distributedCache = distributedCache;
        _providers = providers;
    }

    public async Task<IReadOnlyList<ClientSideTrackingViewModel>?> GetViewModelsAsync()
    {
        var cachedViewModels = await _distributedCache.GetStringAsync(DistributedCacheKey);
        if (!string.IsNullOrEmpty(cachedViewModels) &&
            JsonSerializer.Deserialize<List<ClientSideTrackingViewModel>>(cachedViewModels, _jsonSerializerOptions) is { } viewModels)
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

        await _distributedCache.SetStringAsync(
            DistributedCacheKey,
            JsonSerializer.Serialize(builtViewModels, _jsonSerializerOptions));

        return builtViewModels;
    }

    public Task InvalidateCachedViewModelAsync() => _distributedCache.RemoveAsync(DistributedCacheKey);
}

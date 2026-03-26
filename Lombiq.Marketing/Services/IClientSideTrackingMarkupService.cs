using Lombiq.Marketing.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Services;

/// <summary>
/// Builds and caches the client-side tracking markup rendered by the marketing module.
/// </summary>
public interface IClientSideTrackingMarkupService
{
    /// <summary>
    /// Gets the client-side tracking view models.
    /// </summary>
    /// <returns>The rendered view models.</returns>
    Task<IReadOnlyList<ClientSideTrackingViewModel>> GetViewModelsAsync();

    /// <summary>
    /// Invalidates the cached client-side tracking output.
    /// </summary>
    Task InvalidateCachedViewModelAsync();
}

using Lombiq.Marketing.Models;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Services;

/// <summary>
/// Builds and caches the client-side tracking markup rendered by the marketing module.
/// </summary>
public interface IClientSideTrackingMarkupService
{
    /// <summary>
    /// Gets the client-side tracking view model.
    /// </summary>
    /// <returns>The rendered view model, or <see langword="null"/> if there is nothing to render.</returns>
    Task<ClientSideTrackingViewModel?> GetViewModelAsync();

    /// <summary>
    /// Invalidates the cached client-side tracking output.
    /// </summary>
    Task InvalidateCachedViewModelAsync();
}

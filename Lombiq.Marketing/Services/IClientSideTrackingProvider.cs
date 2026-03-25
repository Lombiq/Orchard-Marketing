using System.Threading.Tasks;

namespace Lombiq.Marketing.Services;

/// <summary>
/// Produces client-side tracking markup for a marketing provider.
/// </summary>
public interface IClientSideTrackingProvider
{
    /// <summary>
    /// Builds the HTML markup to render for client-side tracking.
    /// </summary>
    /// <returns>The HTML markup to render, or <see langword="null"/> if the provider has nothing to output.</returns>
    Task<string?> GetClientSideTrackingMarkupAsync();
}

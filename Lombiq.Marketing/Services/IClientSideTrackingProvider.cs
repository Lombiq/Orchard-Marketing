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

    /// <summary>
    /// Gets the layout zone where the client-side tracking shape should be injected automatically.
    /// </summary>
    /// <returns>The zone name, or <see langword="null"/> to use the default zone.</returns>
    Task<string> GetClientSideTrackingZoneAsync();
}

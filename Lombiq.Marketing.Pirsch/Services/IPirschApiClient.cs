using Lombiq.Marketing.Pirsch.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Services;

/// <summary>
/// Sends write-only tracking requests to the Pirsch API.
/// </summary>
public interface IPirschApiClient
{
    /// <summary>
    /// Sends a hit to the Pirsch API.
    /// </summary>
    /// <param name="request">The hit request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the hit was sent successfully. Otherwise <see langword="false"/>.</returns>
    Task<bool> SendHitAsync(PirschHitRequest request, CancellationToken cancellationToken = default);
}

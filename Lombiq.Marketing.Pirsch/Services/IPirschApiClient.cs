using Lombiq.Marketing.Pirsch.Models;
using System.Net.Http;
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

    /// <summary>
    /// Sends a hit to the Pirsch API and returns the raw HTTP response.
    /// </summary>
    /// <param name="request">The hit request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response from the Pirsch API or a synthetic failure response.</returns>
    Task<HttpResponseMessage> SendHitResponseAsync(PirschHitRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an event to the Pirsch API.
    /// </summary>
    /// <param name="request">The event request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the event was sent successfully. Otherwise <see langword="false"/>.</returns>
    Task<bool> SendEventAsync(PirschEventRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an event to the Pirsch API and returns the raw HTTP response.
    /// </summary>
    /// <param name="request">The event request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response from the Pirsch API or a synthetic failure response.</returns>
    Task<HttpResponseMessage> SendEventResponseAsync(PirschEventRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a session keep-alive request to the Pirsch API.
    /// </summary>
    /// <param name="request">The session request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the request was sent successfully. Otherwise <see langword="false"/>.</returns>
    Task<bool> KeepSessionAliveAsync(PirschSessionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a session keep-alive request to the Pirsch API and returns the raw HTTP response.
    /// </summary>
    /// <param name="request">The session request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response from the Pirsch API or a synthetic failure response.</returns>
    Task<HttpResponseMessage> KeepSessionAliveResponseAsync(
        PirschSessionRequest request,
        CancellationToken cancellationToken = default);
}

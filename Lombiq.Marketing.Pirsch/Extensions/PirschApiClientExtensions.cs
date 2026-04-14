using Lombiq.Marketing.Pirsch.Constants;
using Lombiq.Marketing.Pirsch.Models;
using Lombiq.Marketing.Pirsch.Services;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Services;

public static class PirschApiClientExtensions
{
    /// <summary>
    /// Sends a hit to the Pirsch API.
    /// </summary>
    /// <param name="pirschApiClient">The Pirsch API client.</param>
    /// <param name="request">The hit request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the hit was sent successfully. Otherwise <see langword="false"/>.</returns>
    public static async Task<bool> SendHitAsync(
        this IPirschApiClient pirschApiClient,
        PirschHitRequest request,
        CancellationToken cancellationToken = default)
    {
        using var responseMessage = await pirschApiClient.SendHitResponseAsync(request, cancellationToken);
        return responseMessage.IsSuccessStatusCode;
    }

    /// <summary>
    /// Sends a hit to the Pirsch API and returns the raw HTTP response.
    /// </summary>
    /// <param name="pirschApiClient">The Pirsch API client.</param>
    /// <param name="request">The hit request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response from the Pirsch API or a synthetic failure response.</returns>
    public static Task<HttpResponseMessage> SendHitResponseAsync(
        this IPirschApiClient pirschApiClient,
        PirschHitRequest request,
        CancellationToken cancellationToken = default) =>
        pirschApiClient.SendAsync(PirschApiConstants.HitEndpointPath, request, cancellationToken);

    /// <summary>
    /// Sends an event to the Pirsch API.
    /// </summary>
    /// <param name="pirschApiClient">The Pirsch API client.</param>
    /// <param name="request">The event request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the event was sent successfully. Otherwise <see langword="false"/>.</returns>
    public static async Task<bool> SendEventAsync(
        this IPirschApiClient pirschApiClient,
        PirschEventRequest request,
        CancellationToken cancellationToken = default)
    {
        using var responseMessage = await pirschApiClient.SendEventResponseAsync(request, cancellationToken);
        return responseMessage.IsSuccessStatusCode;
    }

    /// <summary>
    /// Sends an event to the Pirsch API and returns the raw HTTP response.
    /// </summary>
    /// <param name="pirschApiClient">The Pirsch API client.</param>
    /// <param name="request">The event request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response from the Pirsch API or a synthetic failure response.</returns>
    public static Task<HttpResponseMessage> SendEventResponseAsync(
        this IPirschApiClient pirschApiClient,
        PirschEventRequest request,
        CancellationToken cancellationToken = default) =>
        pirschApiClient.SendAsync(PirschApiConstants.EventEndpointPath, request, cancellationToken);

    /// <summary>
    /// Sends a session keep-alive request to the Pirsch API.
    /// </summary>
    /// <param name="pirschApiClient">The Pirsch API client.</param>
    /// <param name="request">The session request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the request was sent successfully. Otherwise <see langword="false"/>.</returns>
    public static async Task<bool> KeepSessionAliveAsync(
        this IPirschApiClient pirschApiClient,
        PirschSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var responseMessage = await pirschApiClient.KeepSessionAliveResponseAsync(request, cancellationToken);
        return responseMessage.IsSuccessStatusCode;
    }

    /// <summary>
    /// Sends a session keep-alive request to the Pirsch API and returns the raw HTTP response.
    /// </summary>
    /// <param name="pirschApiClient">The Pirsch API client.</param>
    /// <param name="request">The session request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response from the Pirsch API or a synthetic failure response.</returns>
    public static Task<HttpResponseMessage> KeepSessionAliveResponseAsync(
        this IPirschApiClient pirschApiClient,
        PirschSessionRequest request,
        CancellationToken cancellationToken = default) =>
        pirschApiClient.SendAsync(PirschApiConstants.SessionEndpointPath, request, cancellationToken);
}

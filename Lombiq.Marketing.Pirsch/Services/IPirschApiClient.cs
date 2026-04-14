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
    /// Sends a write-only request to the specified Pirsch API endpoint and returns the raw HTTP response.
    /// </summary>
    /// <typeparam name="TRequest">The request payload type.</typeparam>
    /// <param name="requestUri">The Pirsch API endpoint path, such as <c>/api/v1/hit</c>.</param>
    /// <param name="request">The request payload to serialize as JSON.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response from the Pirsch API or a synthetic failure response.</returns>
    Task<HttpResponseMessage> SendAsync<TRequest>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken);
}

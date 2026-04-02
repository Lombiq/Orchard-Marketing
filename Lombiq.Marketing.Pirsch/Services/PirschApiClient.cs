using Lombiq.Marketing.Pirsch.Constants;
using Lombiq.Marketing.Pirsch.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Services;

public sealed class PirschApiClient : IPirschApiClient
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _httpClient;
    private readonly IOptionsSnapshot<PirschSettings> _pirschSettingsOptions;
    private readonly ILogger<PirschApiClient> _logger;

    public PirschApiClient(
        HttpClient httpClient,
        IOptionsSnapshot<PirschSettings> pirschSettingsOptions,
        ILogger<PirschApiClient> logger)
    {
        _httpClient = httpClient;
        _pirschSettingsOptions = pirschSettingsOptions;
        _logger = logger;
    }

    public async Task<bool> SendHitAsync(PirschHitRequest request, CancellationToken cancellationToken = default)
    {
        using var responseMessage = await SendHitResponseAsync(request, cancellationToken);
        return responseMessage.IsSuccessStatusCode;
    }

    public Task<HttpResponseMessage> SendHitResponseAsync(
        PirschHitRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync(PirschApiConstants.HitEndpointPath, request, cancellationToken);

    public async Task<bool> SendEventAsync(PirschEventRequest request, CancellationToken cancellationToken = default)
    {
        using var responseMessage = await SendEventResponseAsync(request, cancellationToken);
        return responseMessage.IsSuccessStatusCode;
    }

    public Task<HttpResponseMessage> SendEventResponseAsync(
        PirschEventRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync(PirschApiConstants.EventEndpointPath, request, cancellationToken);

    public async Task<bool> KeepSessionAliveAsync(PirschSessionRequest request, CancellationToken cancellationToken = default)
    {
        using var responseMessage = await KeepSessionAliveResponseAsync(request, cancellationToken);
        return responseMessage.IsSuccessStatusCode;
    }

    public Task<HttpResponseMessage> KeepSessionAliveResponseAsync(
        PirschSessionRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync(PirschApiConstants.SessionEndpointPath, request, cancellationToken);

    private async Task<HttpResponseMessage> SendAsync<TRequest>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var accessKey = _pirschSettingsOptions.Value.ClientSecret;
        if (string.IsNullOrWhiteSpace(accessKey))
        {
            _logger.LogError("Cannot send a request to Pirsch API because the client secret is not configured");
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
        requestMessage.Content = JsonContent.Create(request, options: _jsonSerializerOptions);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessKey);

        try
        {
            return await _httpClient.SendAsync(requestMessage, cancellationToken);
        }
        catch (HttpRequestException httpRequestException)
        {
            _logger.LogError(httpRequestException, "There was a problem sending the request to the Pirsch API");
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}

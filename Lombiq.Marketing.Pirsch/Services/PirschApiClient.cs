using Lombiq.Marketing.Pirsch.Constants;
using Lombiq.Marketing.Pirsch.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    public Task<bool> SendHitAsync(PirschHitRequest request, CancellationToken cancellationToken = default) =>
        SendAsync(PirschApiConstants.HitEndpointPath, request, cancellationToken);

    private async Task<bool> SendAsync<TRequest>(string requestUri, TRequest request, CancellationToken cancellationToken)
    {
        var accessKey = _pirschSettingsOptions.Value.ClientSecret;
        if (string.IsNullOrWhiteSpace(accessKey))
        {
            _logger.LogError("Cannot send a request to Pirsch API because the client secret is not configured");
            return false;
        }

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
        requestMessage.Content = JsonContent.Create(request, options: _jsonSerializerOptions);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessKey);

        try
        {
            using var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);
            responseMessage.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "Successfully sent a request to Pirsch API. Request URI: {RequestUri}",
                requestUri);
        }
        catch (HttpRequestException httpRequestException)
        {
            _logger.LogError(httpRequestException, "There was a problem sending the request to the Pirsch API");
            return false;
        }

        return true;
    }
}

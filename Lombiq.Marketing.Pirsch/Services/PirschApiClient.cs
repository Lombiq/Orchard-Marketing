using Lombiq.Marketing.Pirsch.Constants;
using Lombiq.Marketing.Pirsch.Models;
using Microsoft.Extensions.Options;
using System;
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

    public PirschApiClient(HttpClient httpClient, IOptionsSnapshot<PirschSettings> pirschSettingsOptions)
    {
        _httpClient = httpClient;
        _pirschSettingsOptions = pirschSettingsOptions;
    }

    public Task SendHitAsync(PirschHitRequest request, CancellationToken cancellationToken = default) =>
        SendAsync(PirschApiConstants.HitEndpointPath, request, cancellationToken);

    private async Task SendAsync<TRequest>(string requestUri, TRequest request, CancellationToken cancellationToken)
    {
        var accessKey = _pirschSettingsOptions.Value.ClientSecret;
        if (string.IsNullOrWhiteSpace(accessKey))
        {
            throw new InvalidOperationException("Pirsch access key is not configured.");
        }

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
        requestMessage.Content = JsonContent.Create(request, options: _jsonSerializerOptions);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessKey);

        using var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);
        responseMessage.EnsureSuccessStatusCode();
    }
}

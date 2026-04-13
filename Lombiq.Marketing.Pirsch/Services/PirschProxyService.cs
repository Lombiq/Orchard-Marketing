using Lombiq.Marketing.Pirsch.Constants;
using Lombiq.Marketing.Pirsch.Extensions;
using Lombiq.Marketing.Pirsch.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Services;

public class PirschProxyService : IPirschProxyService
{
    private const string CacheKey = "Lombiq.Marketing.Pirsch.ProxyScript";

    private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    private readonly IHttpContextAccessor _hca;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PirschProxyService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IPirschApiClient _pirschApiClient;

    public PirschProxyService(
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        ILogger<PirschProxyService> logger,
        IMemoryCache memoryCache,
        IPirschApiClient pirschApiClient)
    {
        _hca = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _memoryCache = memoryCache;
        _pirschApiClient = pirschApiClient;
    }

    public async Task ProxyScriptAsync()
    {
        ArgumentNullException.ThrowIfNull(_hca.HttpContext);

        if (_memoryCache.TryGetValue(CacheKey, out CachedPirschScript? cachedScript) && cachedScript != null)
        {
            await WriteResponseAsync(
                _hca.HttpContext.Response,
                StatusCodes.Status200OK,
                cachedScript.ContentType,
                cachedScript.Content,
                _hca.HttpContext.RequestAborted);
            return;
        }

        try
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, CreatePirschUri(PirschProxyConstants.PirschScriptPath));
            using var responseMessage = await _httpClientFactory
                .CreateClient(nameof(IPirschProxyService))
                .SendAsync(requestMessage, _hca.HttpContext.RequestAborted);

            if (!responseMessage.IsSuccessStatusCode)
            {
                await WriteResponseAsync(_hca.HttpContext.Response, responseMessage, _hca.HttpContext.RequestAborted);
                return;
            }

            cachedScript = new CachedPirschScript(
                await responseMessage.Content.ReadAsStringAsync(_hca.HttpContext.RequestAborted),
                responseMessage.Content.Headers.ContentType?.ToString() ?? "application/javascript");

            _memoryCache.Set(CacheKey, cachedScript, _cacheDuration);
            await WriteResponseAsync(
                _hca.HttpContext.Response,
                StatusCodes.Status200OK,
                cachedScript.ContentType,
                cachedScript.Content,
                _hca.HttpContext.RequestAborted);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Failed to proxy the Pirsch script request for path {RequestPath}.",
                _hca.HttpContext.Request.Path);

            _hca.HttpContext.Response.StatusCode = StatusCodes.Status502BadGateway;
        }
    }

    public Task ProxyPageViewAsync() =>
        ProxyApiRequestAsync(() => Task.FromResult(CreatePageViewRequest()), _pirschApiClient.SendHitResponseAsync);

    public Task ProxyEventAsync() =>
        ProxyApiRequestAsync(CreateEventRequestAsync, _pirschApiClient.SendEventResponseAsync);

    public Task ProxySessionAsync() =>
        ProxyApiRequestAsync(() => Task.FromResult(CreateSessionRequest()), _pirschApiClient.KeepSessionAliveResponseAsync);

    private async Task ProxyApiRequestAsync<TRequest>(
        Func<Task<TRequest>> createRequestAsync,
        Func<TRequest, CancellationToken, Task<HttpResponseMessage>> sendRequestAsync)
    {
        ArgumentNullException.ThrowIfNull(_hca.HttpContext);

        var context = _hca.HttpContext;

        try
        {
            var request = await createRequestAsync();
            using var responseMessage = await sendRequestAsync(request, context.RequestAborted);
            await WriteResponseAsync(context.Response, responseMessage, context.RequestAborted);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Failed to proxy the Pirsch request for path {RequestPath}.",
                context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status502BadGateway;
        }
    }

    private static Task WriteResponseAsync(
        HttpResponse response,
        HttpResponseMessage responseMessage,
        CancellationToken cancellationToken)
    {
        response.StatusCode = (int)responseMessage.StatusCode;
        response.ContentType = responseMessage.Content.Headers.ContentType?.ToString();

        return responseMessage.Content.CopyToAsync(response.Body, cancellationToken);
    }

    private static Task WriteResponseAsync(
        HttpResponse response,
        int statusCode,
        string? contentType,
        string content,
        CancellationToken cancellationToken)
    {
        response.StatusCode = statusCode;
        response.ContentType = contentType;

        return response.WriteAsync(content, cancellationToken);
    }

    private PirschHitRequest CreatePageViewRequest()
    {
        ArgumentNullException.ThrowIfNull(_hca.HttpContext);

        var hitRequest = new PirschHitRequest();
        ApplyRequestData(hitRequest, includeAcceptLanguage: true);

        var request = _hca.HttpContext.Request;
        hitRequest.Code = request.Query["code"].ToString();
        hitRequest.Url = request.Query["url"].ToString();
        hitRequest.Title = request.Query["t"].ToString();
        hitRequest.Referrer = request.Query["ref"].ToString();
        hitRequest.ScreenWidth = ParseNullableInt(request.Query["w"].ToString());
        hitRequest.ScreenHeight = ParseNullableInt(request.Query["h"].ToString());

        return hitRequest;
    }

    private async Task<PirschEventRequest> CreateEventRequestAsync()
    {
        ArgumentNullException.ThrowIfNull(_hca.HttpContext);

        var eventRequest =
            await JsonSerializer.DeserializeAsync<PirschEventRequest>(
                _hca.HttpContext.Request.Body,
                cancellationToken: _hca.HttpContext.RequestAborted) ?? new PirschEventRequest();
        ApplyRequestData(eventRequest, includeAcceptLanguage: true);

        return eventRequest;
    }

    private PirschSessionRequest CreateSessionRequest()
    {
        var sessionRequest = new PirschSessionRequest();
        ApplyRequestData(sessionRequest, includeAcceptLanguage: false);

        return sessionRequest;
    }

    private void ApplyRequestData(PirschRequestData requestData, bool includeAcceptLanguage)
    {
        ArgumentNullException.ThrowIfNull(_hca.HttpContext);

        var request = _hca.HttpContext.Request;
        requestData.Ip = GetIp(request);
        requestData.UserAgent = request.Headers.UserAgent.ToString();
        requestData.SecChUa = request.Headers["Sec-CH-UA"].ToString();
        requestData.SecChUaMobile = request.Headers["Sec-CH-UA-Mobile"].ToString();
        requestData.SecChUaPlatform = request.Headers["Sec-CH-UA-Platform"].ToString();
        requestData.SecChUaPlatformVersion = request.Headers["Sec-CH-UA-Platform-Version"].ToString();
        requestData.SecChWidth = request.Headers["Sec-CH-Width"].ToString();
        requestData.SecChViewportWidth = request.Headers["Sec-CH-Viewport-Width"].ToString();

        if (includeAcceptLanguage)
        {
            requestData.AcceptLanguage = request.Headers.AcceptLanguage.ToString();
        }
    }

    private static string? GetIp(HttpRequest request)
    {
        var cloudflareIp = request.Headers["CF-Connecting-IP"].ToString();
        return !string.IsNullOrWhiteSpace(cloudflareIp)
            ? cloudflareIp
            : request.HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static int? ParseNullableInt(string value) =>
        int.TryParse(value, out var parsedValue) ? parsedValue : null;

    private static Uri CreatePirschUri(string path) => new(PirschProxyConstants.PirschBaseUrl + path);

    private sealed record CachedPirschScript(string Content, string ContentType);
}

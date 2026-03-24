using Lombiq.Marketing.Pirsch.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Middlewares;

public sealed class PirschProxyMiddleware
{
    private const string CacheKey = "Lombiq.Marketing.Pirsch.ProxyScript";
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    private readonly ILogger<PirschProxyMiddleware> _logger;
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _memoryCache;

    public PirschProxyMiddleware(ILogger<PirschProxyMiddleware> logger, RequestDelegate next, IMemoryCache memoryCache)
    {
        _logger = logger;
        _next = next;
        _memoryCache = memoryCache;
    }

    public async Task InvokeAsync(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        if (context.Request.Path != PirschProxyConstants.ProxyScriptPath || !HttpMethods.IsGet(context.Request.Method))
        {
            await _next(context);
            return;
        }

        if (_memoryCache.TryGetValue(CacheKey, out CachedPirschScript? cachedScript) && cachedScript != null)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = cachedScript.ContentType;
            await context.Response.WriteAsync(cachedScript.Content, context.RequestAborted);
            return;
        }

        using var requestMessage = new HttpRequestMessage(
            new HttpMethod(context.Request.Method),
            PirschProxyConstants.PirschScriptUrl);

        try
        {
            using var responseMessage = await httpClientFactory
                .CreateClient(nameof(PirschProxyMiddleware))
                .SendAsync(requestMessage, context.RequestAborted);

            responseMessage.EnsureSuccessStatusCode();

            context.Response.StatusCode = (int)responseMessage.StatusCode;

            var content = await responseMessage.Content.ReadAsStringAsync(context.RequestAborted);

            cachedScript = new CachedPirschScript(
                content,
                responseMessage.Content.Headers.ContentType?.ToString() ?? "application/javascript");

            _memoryCache.Set(CacheKey, cachedScript, _cacheDuration);

            context.Response.ContentType = cachedScript.ContentType;
            await context.Response.WriteAsync(cachedScript.Content, context.RequestAborted);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Failed to proxy the Pirsch script for path {RequestPath}", context.Request.Path);
        }
    }

    private sealed record CachedPirschScript(string Content, string ContentType);
}

using Lombiq.Marketing.Pirsch.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Middlewares;

public sealed class PirschProxyMiddleware
{
    private readonly ILogger<PirschProxyMiddleware> _logger;
    private readonly RequestDelegate _next;

    public PirschProxyMiddleware(ILogger<PirschProxyMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        if (context.Request.Path != PirschProxyConstants.ProxyScriptPath || !HttpMethods.IsGet(context.Request.Method))
        {
            await _next(context);
            return;
        }

        using var requestMessage = new HttpRequestMessage(
            new HttpMethod(context.Request.Method),
            PirschProxyConstants.PirschScriptUrl);

        foreach (var header in context.Request.Headers)
        {
            if (header.Key.EqualsOrdinalIgnoreCase("Host")) continue;

            requestMessage.Headers.TryAddWithoutValidation(header.Key, [.. header.Value]);
        }

        try
        {
            using var responseMessage = await httpClientFactory
                .CreateClient(nameof(PirschProxyMiddleware))
                .SendAsync(requestMessage, context.RequestAborted);

            context.Response.StatusCode = (int)responseMessage.StatusCode;

            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            await responseMessage.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Failed to proxy the Pirsch script for path {RequestPath}", context.Request.Path);
        }
    }
}

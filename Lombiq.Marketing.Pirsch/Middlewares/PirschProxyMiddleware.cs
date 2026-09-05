using Lombiq.Marketing.Pirsch.Constants;
using Lombiq.Marketing.Pirsch.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Middlewares;

public sealed class PirschProxyMiddleware
{
    private readonly RequestDelegate _next;

    public PirschProxyMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context, IPirschProxyService pirschProxyService)
    {
        if (!context.Request.Path.StartsWithSegments(PirschProxyConstants.ProxyPathPrefix))
        {
            return _next(context);
        }

        var path = context.Request.Path.ToString();

        if (path.EndsWithOrdinalIgnoreCase(PirschProxyConstants.ProxyScriptPath)) { return pirschProxyService.ProxyScriptAsync(); }
        if (path.EndsWithOrdinalIgnoreCase(PirschProxyConstants.ProxyPageViewPath)) { return pirschProxyService.ProxyPageViewAsync(); }
        if (path.EndsWithOrdinalIgnoreCase(PirschProxyConstants.ProxyEventPath)) { return pirschProxyService.ProxyEventAsync(); }
        if (path.EndsWithOrdinalIgnoreCase(PirschProxyConstants.ProxySessionPath)) { return pirschProxyService.ProxySessionAsync(); }

        return context.NotFoundAsync();
    }
}

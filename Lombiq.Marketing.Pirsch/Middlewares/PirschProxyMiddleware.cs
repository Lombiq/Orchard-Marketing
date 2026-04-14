using Lombiq.Marketing.Pirsch.Constants;
using Lombiq.Marketing.Pirsch.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Middlewares;

public sealed class PirschProxyMiddleware
{
    private readonly RequestDelegate _next;

    public PirschProxyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IPirschProxyService pirschProxyService)
    {
        if (!context.Request.Path.StartsWithSegments(PirschProxyConstants.ProxyPathPrefix))
        {
            await _next(context);
            return;
        }

        if (context.Request.Path == PirschProxyConstants.ProxyScriptPath)
        {
            await pirschProxyService.ProxyScriptAsync();
            return;
        }

        if (context.Request.Path == PirschProxyConstants.ProxyPageViewPath)
        {
            await pirschProxyService.ProxyPageViewAsync();
            return;
        }

        if (context.Request.Path == PirschProxyConstants.ProxyEventPath)
        {
            await pirschProxyService.ProxyEventAsync();
            return;
        }

        if (context.Request.Path == PirschProxyConstants.ProxySessionPath)
        {
            await pirschProxyService.ProxySessionAsync();
            return;
        }

        context.Response.StatusCode = StatusCodes.Status404NotFound;
    }
}

using Lombiq.Marketing.Pirsch.Constants;
using Lombiq.Marketing.Pirsch.Services;
using Microsoft.AspNetCore.Http;
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

        return context.Request.Path.ToString() switch
        {
            PirschProxyConstants.ProxyScriptPath => pirschProxyService.ProxyScriptAsync(),
            PirschProxyConstants.ProxyPageViewPath => pirschProxyService.ProxyPageViewAsync(),
            PirschProxyConstants.ProxyEventPath => pirschProxyService.ProxyEventAsync(),
            PirschProxyConstants.ProxySessionPath => pirschProxyService.ProxySessionAsync(),
            _ => context.NotFoundAsync(),
        };
    }
}

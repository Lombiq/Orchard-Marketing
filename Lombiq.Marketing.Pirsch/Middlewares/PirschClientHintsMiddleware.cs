using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Middlewares;

public sealed class PirschClientHintsMiddleware
{
    private const string AcceptChHeaderName = "Accept-CH";
    private const string PermissionsPolicyHeaderName = "Permissions-Policy";

    // The AcceptChHeaderValue and PermissionsPolicyHeaderValue values might change. Update them from
    // https://docs.pirsch.io/get-started/client-hints if necessary.
    private const string AcceptChHeaderValue =
        "Sec-CH-UA, Sec-CH-UA-Mobile, Sec-CH-UA-Platform, Sec-CH-UA-Platform-Version, Sec-CH-Width, Sec-CH-Viewport-Width, Width, Viewport-Width";

    private const string PermissionsPolicyHeaderValue =
        @"ch-ua=(self ""https://api.pirsch.io""), ch-ua-mobile=(self ""https://api.pirsch.io""), " +
        @"ch-ua-platform=(self ""https://api.pirsch.io""), ch-ua-platform-version=(self ""https://api.pirsch.io""), " +
        @"ch-width=(self ""https://api.pirsch.io""), ch-viewport-width=(self ""https://api.pirsch.io""), " +
        @"width=(self ""https://api.pirsch.io""), viewport-width=(self ""https://api.pirsch.io"")";

    private readonly RequestDelegate _next;

    public PirschClientHintsMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.IsAdminUrl())
        {
            await _next(context);
            return;
        }

        context.Response.OnStarting(() =>
        {
            AppendHeader(context.Response.Headers, AcceptChHeaderName, AcceptChHeaderValue);
            AppendHeader(context.Response.Headers, PermissionsPolicyHeaderName, PermissionsPolicyHeaderValue);

            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static void AppendHeader(IHeaderDictionary headers, string headerName, string headerValue) =>
        headers[headerName] = headers.TryGetValue(headerName, out var existingHeaderValue) && !string.IsNullOrWhiteSpace(existingHeaderValue)
            ? $"{existingHeaderValue}, {headerValue}"
            : headerValue;
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;

namespace Lombiq.Marketing.UrlShortener.Helpers;

public static class ShortUrlHelpers
{
    public static Uri? GetFullShortUrl(string? shortUrl, HttpContext? httpContext)
    {
        if (string.IsNullOrWhiteSpace(shortUrl))
        {
            return null;
        }

        return httpContext == null
            ? new Uri(shortUrl, UriKind.RelativeOrAbsolute)
            : new Uri(
                UriHelper.BuildAbsolute(
                    httpContext.Request.Scheme,
                    httpContext.Request.Host,
                    httpContext.Request.PathBase,
                    shortUrl),
                UriKind.Absolute);
    }
}

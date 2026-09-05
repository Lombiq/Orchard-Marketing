using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Parser;
using Lombiq.Marketing.Pirsch.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lombiq.Marketing.Pirsch.Services;

public static class PirschSettingsSanitizer
{
    private static readonly PirschScriptMarkupFormatter _formatter = new();

    // These are all the possible attributes from https://dashboard.pirsch.io/settings/integration, if you select all
    // the options under Snippet Show Advanced Options.
    private static readonly HashSet<string> _allowedAttributeNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "defer",
        "src",
        "id",
        "data-code",
        "data-dev",
        "data-hit-endpoint",
        "data-event-endpoint",
        "data-session-endpoint",
        "data-path-prefix",
        "data-disable-page-views",
        "data-disable-query",
        "data-disable-referrer",
        "data-disable-resolution",
        "data-disable-history",
        "data-disable-outbound-links",
        "data-disable-downloads",
        "data-strip-anchor",
        "data-enable-session",
        "data-outbound-link-event-name",
        "data-download-event-name",
        "data-not-found-event-name",
        "data-download-extensions",
        "data-include",
        "data-exclude",
        "data-domain",
    };

    public static string SanitizeClientSideCodeSnippet(string? snippetHtml) =>
        SanitizeClientSideCodeSnippet(snippetHtml, httpContext: null);

    public static string SanitizeClientSideCodeSnippet(string? snippetHtml, HttpContext? httpContext)
    {
        static string GetUri(IUrlHelper? urlHelper, string path) =>
            urlHelper?.Content('~' + path) is { Length: > 0 } url ? url : path; 

        if (string.IsNullOrWhiteSpace(snippetHtml)) return string.Empty;

        var script = new HtmlParser()
            .ParseDocument(snippetHtml)
            .Scripts
            .FirstOrDefault();

        if (script is null) return string.Empty;

        var removeAttributes = script.Attributes
            .Where(attribute => !_allowedAttributeNames.Contains(attribute.Name))
            .Select(attribute => attribute.Name)
            .ToArray();

        foreach (var attributeName in removeAttributes) script.RemoveAttribute(attributeName);

        IUrlHelper? urlHelper = null;
        if (httpContext != null)
        {
            var routeData = new RouteData();
            routeData.Routers.Add(new RouteCollection());
            var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());

            var factory = httpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            urlHelper = factory.GetUrlHelper(actionContext);
        }

        SetProxyAttribute(script, "src", GetUri(urlHelper, PirschProxyConstants.ProxyScriptPath));
        SetProxyAttribute(script, "data-hit-endpoint", GetUri(urlHelper, PirschProxyConstants.ProxyPageViewPath));
        SetProxyAttribute(script, "data-event-endpoint", GetUri(urlHelper, PirschProxyConstants.ProxyEventPath));
        SetProxyAttribute(script, "data-session-endpoint", GetUri(urlHelper, PirschProxyConstants.ProxySessionPath));
        script.TextContent = string.Empty;

        return SerializeScript(script);
    }

    public static string SerializeScript(IElement script)
    {
        using var stringWriter = new StringWriter();
        script.ToHtml(stringWriter, _formatter);

        return stringWriter.ToString();
    }

    private static void SetProxyAttribute(IElement script, string attributeName, string attributeValue)
    {
        if (script.GetAttribute(attributeName)?.EqualsOrdinalIgnoreCase(attributeValue) != true)
        {
            script.SetAttribute(attributeName, attributeValue);
        }
    }

    private sealed class PirschScriptMarkupFormatter : HtmlMarkupFormatter
    {
        private const string Indentation = "    ";
        private const string NewLine = "\n";

        public override string OpenTag(IElement element, bool selfClosing)
        {
            if (!element.LocalName.EqualsOrdinalIgnoreCase("script"))
            {
                return base.OpenTag(element, selfClosing);
            }

            var attributes = element.Attributes.Select(Attribute).ToArray();
            if (attributes.Length == 0) return "<script>";

            return "<script" + NewLine + Indentation + string.Join(NewLine + Indentation, attributes) + ">";
        }
    }
}

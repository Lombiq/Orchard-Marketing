using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lombiq.Marketing.Services;

public static class PirschSettingsSanitizer
{
    private const string ProxyScriptSource = "/secret-sauce/sauce.js";
    private static readonly PirschScriptMarkupFormatter _formatter = new();

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

    public static string SanitizeClientSideCodeSnippet(string snippetHtml)
    {
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

        if (!string.Equals(script.GetAttribute("src"), ProxyScriptSource, StringComparison.OrdinalIgnoreCase))
        {
            script.SetAttribute("src", ProxyScriptSource);
        }

        script.TextContent = string.Empty;

        using var stringWriter = new StringWriter();
        script.ToHtml(stringWriter, _formatter);

        return stringWriter.ToString();
    }

    private sealed class PirschScriptMarkupFormatter : HtmlMarkupFormatter
    {
        private const string Indentation = "    ";
        private const string NewLine = "\n";

        public override string OpenTag(IElement element, bool selfClosing)
        {
            if (!element.LocalName.Equals("script", StringComparison.OrdinalIgnoreCase))
            {
                return base.OpenTag(element, selfClosing);
            }

            var attributes = element.Attributes.Select(Attribute).ToArray();
            if (attributes.Length == 0) return "<script>";

            return "<script" + NewLine + Indentation + string.Join(NewLine + Indentation, attributes) + ">";
        }
    }
}

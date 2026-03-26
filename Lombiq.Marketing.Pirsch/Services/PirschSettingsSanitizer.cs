using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Parser;
using Lombiq.Marketing.Pirsch.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lombiq.Marketing.Pirsch.Services;

public static class PirschSettingsSanitizer
{
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

    public static string SanitizeClientSideCodeSnippet(string? snippetHtml)
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

        if (!script.GetAttribute("src")?.EqualsOrdinalIgnoreCase(PirschProxyConstants.ProxyScriptPath) == true)
        {
            script.SetAttribute("src", PirschProxyConstants.ProxyScriptPath);
        }

        script.TextContent = string.Empty;

        return SerializeScript(script);
    }

    public static string SerializeScript(IElement script)
    {
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

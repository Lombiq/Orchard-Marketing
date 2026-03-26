using Lombiq.Marketing.Pirsch.Models;
using Lombiq.Marketing.Services;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Services;

public sealed class PirschClientSideTrackingProvider : IClientSideTrackingProvider
{
    private readonly IHostEnvironment _environment;
    private readonly IOptionsSnapshot<PirschSettings> _pirschSettingsOptions;

    public PirschClientSideTrackingProvider(
        IHostEnvironment environment,
        IOptionsSnapshot<PirschSettings> pirschSettingsOptions)
    {
        _environment = environment;
        _pirschSettingsOptions = pirschSettingsOptions;
    }

    public async Task<string?> GetClientSideTrackingMarkupAsync()
    {
        var settings = _pirschSettingsOptions.Value;
        var snippetHtml = settings.ClientSideCodeSnippet;
        var renderedSnippet = snippetHtml;

        if (_environment.IsDevelopment() && !string.IsNullOrWhiteSpace(snippetHtml))
        {
            var parser = new HtmlParser();
            var script = (await parser.ParseDocumentAsync(snippetHtml)).Scripts.FirstOrDefault();

            if (script != null)
            {
                var fallbackDataDev = settings.DataDev;

                if (!string.IsNullOrEmpty(script.GetAttribute("data-dev")))
                {
                    renderedSnippet = PirschSettingsSanitizer.SerializeScript(script);
                }
                else if (string.IsNullOrEmpty(script.GetAttribute("data-dev")) &&
                    !string.IsNullOrWhiteSpace(fallbackDataDev))
                {
                    script.SetAttribute("data-dev", fallbackDataDev);
                    renderedSnippet = PirschSettingsSanitizer.SerializeScript(script);
                }
            }
        }

        return renderedSnippet;
    }

    public Task<string?> GetClientSideTrackingZoneAsync() =>
        Task.FromResult<string?>(_pirschSettingsOptions.Value.AutoRenderZone);
}

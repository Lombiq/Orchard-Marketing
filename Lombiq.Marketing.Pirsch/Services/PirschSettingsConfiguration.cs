using Lombiq.Marketing.Pirsch.Models;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace Lombiq.Marketing.Pirsch.Services;

public sealed class PirschSettingsConfiguration : IConfigureOptions<PirschSettings>
{
    private readonly ISiteService _siteService;

    public PirschSettingsConfiguration(ISiteService siteService) => _siteService = siteService;

    public void Configure(PirschSettings options)
    {
        var settings = _siteService.GetSettingsAsync<PirschSettings>().GetAwaiter().GetResult();

        if (!string.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            options.ClientSecret = settings.ClientSecret;
        }

        options.ClientSideCodeSnippet = !string.IsNullOrWhiteSpace(settings.ClientSideCodeSnippet)
            ? settings.ClientSideCodeSnippet
            : PirschSettingsSanitizer.SanitizeClientSideCodeSnippet(options.ClientSideCodeSnippet);

        if (!string.IsNullOrWhiteSpace(settings.DataDev))
        {
            options.DataDev = settings.DataDev;
        }

        if (!string.IsNullOrWhiteSpace(settings.AutoRenderZone))
        {
            options.AutoRenderZone = settings.AutoRenderZone;
        }
    }
}

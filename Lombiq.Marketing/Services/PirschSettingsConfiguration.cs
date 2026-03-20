using Lombiq.Marketing.Models;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;
using System;

namespace Lombiq.Marketing.Services;

public sealed class PirschSettingsConfiguration : IConfigureOptions<PirschSettings>
{
    private readonly ISiteService _siteService;

    public PirschSettingsConfiguration(ISiteService siteService) => _siteService = siteService;

    public void Configure(PirschSettings options)
    {
        var settings = _siteService.GetSettingsAsync<PirschSettings>().GetAwaiter().GetResult();

        if (string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            options.ClientSecret = settings.ClientSecret;
        }

        if (string.IsNullOrWhiteSpace(options.ClientSideCodeSnippet))
        {
            options.ClientSideCodeSnippet = settings.ClientSideCodeSnippet;
        }
    }
}

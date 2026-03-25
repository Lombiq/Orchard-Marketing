using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Lombiq.Marketing.Pirsch.Models;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Maintenance.ClearClientSecrets;

public sealed class ClearPirschClientSecretsMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<ClearPirschClientSecretsMaintenanceOptions> _options;
    private readonly ISiteService _siteService;

    public ClearPirschClientSecretsMaintenanceProvider(
        IOptions<ClearPirschClientSecretsMaintenanceOptions> options,
        ISiteService siteService)
    {
        _options = options;
        _siteService = siteService;
    }

    public override async Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        _options.Value.IsEnabled &&
        !context.WasLatestExecutionSuccessful() &&
        !string.IsNullOrWhiteSpace((await _siteService.GetSettingsAsync<PirschSettings>()).ClientSecret);

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var siteSettings = await _siteService.LoadSiteSettingsAsync();
        siteSettings.Alter<PirschSettings>(nameof(PirschSettings), settings => settings.ClientSecret = string.Empty);

        await _siteService.UpdateSiteSettingsAsync(siteSettings);
    }
}

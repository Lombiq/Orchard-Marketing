using Lombiq.Marketing.Constants;
using Lombiq.Marketing.Drivers;
using Lombiq.Marketing.Models;
using Lombiq.Marketing.Navigation;
using Lombiq.Marketing.Permissions;
using Lombiq.Marketing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace Lombiq.Marketing;

[Feature(FeatureIds.Base)]
public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<PirschSettings>(_shellConfiguration.GetSection("Lombiq_Marketing"));
        services.AddTransient<IConfigureOptions<PirschSettings>, PirschSettingsConfiguration>();
        services.AddSiteDisplayDriver<PirschSettingsDriver>();
        services.AddPermissionProvider<PirschSettingsPermissions>();
        services.AddNavigationProvider<PirschSettingsAdminMenu>();
    }
}

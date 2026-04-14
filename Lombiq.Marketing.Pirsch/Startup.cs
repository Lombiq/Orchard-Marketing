using Lombiq.Marketing.Pirsch.Constants;
using Lombiq.Marketing.Pirsch.Drivers;
using Lombiq.Marketing.Pirsch.Middlewares;
using Lombiq.Marketing.Pirsch.Models;
using Lombiq.Marketing.Pirsch.Navigation;
using Lombiq.Marketing.Pirsch.Permissions;
using Lombiq.Marketing.Pirsch.Services;
using Lombiq.Marketing.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using System;
using System.Net;
using System.Net.Http;

namespace Lombiq.Marketing.Pirsch;

[Feature(FeatureIds.Base)]
public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<PirschSettings>(_shellConfiguration.GetSection("Lombiq_Marketing:Pirsch"));
        services.AddHttpClient<IPirschApiClient, PirschApiClient>(client => client.BaseAddress = new Uri(PirschApiConstants.BaseUrl));
        services.AddHttpClient(nameof(PirschProxyMiddleware))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // Pirsch uses compression.
                AutomaticDecompression = DecompressionMethods.All,
            });
        services.AddTransient<IConfigureOptions<PirschSettings>, PirschSettingsConfiguration>();
        services.AddScoped<IClientSideTrackingProvider, PirschClientSideTrackingProvider>();
        services.AddScoped<IShortUrlHitHandler, PirschShortUrlHitHandler>();
        services.AddSiteDisplayDriver<PirschSettingsDriver>();
        services.AddPermissionProvider<PirschSettingsPermissions>();
        services.AddNavigationProvider<PirschSettingsAdminMenu>();

        services.AddContentSecurityPolicyProvider<PirschSecurityPolicyProvider>();

        services.AddScoped<IPirschProxyService, PirschProxyService>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        app.UseMiddleware<PirschClientHintsMiddleware>();
        app.UseMiddleware<PirschProxyMiddleware>();
    }
}

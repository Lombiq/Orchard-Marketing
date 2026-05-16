using Lombiq.Marketing.Constants;
using Lombiq.Marketing.Filters;
using Lombiq.Marketing.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace Lombiq.Marketing;

[Feature(FeatureIds.Base)]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IClientSideTrackingMarkupService, ClientSideTrackingMarkupService>();
        services.Configure<MvcOptions>(options => options.Filters.Add<ClientSideTrackingInjectingFilter>());
    }
}

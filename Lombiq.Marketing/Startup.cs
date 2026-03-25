using Lombiq.Marketing.Constants;
using Lombiq.Marketing.Services;
using Lombiq.Marketing.UrlShortener.Events;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace Lombiq.Marketing;

[Feature(FeatureIds.Base)]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IClientSideTrackingMarkupService, ClientSideTrackingMarkupService>();
        services.AddScoped<IShortUrlRedirectEventHandler, MarketingShortUrlRedirectEventHandler>();
    }
}

using Lombiq.Marketing.Constants;
using Lombiq.Marketing.Services;
using Lombiq.Marketing.UrlShortener.Events;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using UrlShortenerFeatureIds = Lombiq.Marketing.UrlShortener.Constants.FeatureIds;

namespace Lombiq.Marketing;

[Feature(FeatureIds.Base)]
[RequireFeatures(UrlShortenerFeatureIds.Base)]
public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<IShortUrlRedirectEventHandler, MarketingShortUrlRedirectEventHandler>();
}

using Lombiq.Marketing.UrlShortener.Constants;
using Lombiq.Marketing.UrlShortener.Handlers;
using Lombiq.Marketing.UrlShortener.Indexes;
using Lombiq.Marketing.UrlShortener.Migrations;
using Lombiq.Marketing.UrlShortener.Models;
using Lombiq.Marketing.UrlShortener.Navigation;
using Lombiq.Marketing.UrlShortener.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Data;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace Lombiq.Marketing.UrlShortener;

[Feature(FeatureIds.Base)]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPart<UtmPart>();
        services.AddContentPart<ShortUrlPart>()
            .AddHandler<ShortUrlPartHandler>()
            .WithMigration<ShortUrlMigration>();

        services.AddContentPart<AutoroutePart>().AddHandler<AutoroutePartHandler>();

        services.AddIndexProvider<ShortUrlPartIndexProvider>();

        services.AddScoped<IUrlShorteningService, UrlShorteningService>();

        services.AddScoped<INavigationProvider, UrlShortenerAdminMenu>();
    }
}

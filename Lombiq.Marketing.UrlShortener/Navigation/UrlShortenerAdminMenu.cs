using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Lombiq.Marketing.UrlShortener.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Contents.Controllers;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;

namespace Lombiq.Marketing.UrlShortener.Navigation;

public sealed class UrlShortenerAdminMenu : AdminMenuNavigationProviderBase
{
    public UrlShortenerAdminMenu(IHttpContextAccessor hca, IStringLocalizer<UrlShortenerAdminMenu> stringLocalizer)
        : base(hca, stringLocalizer)
    {
    }

    protected override void Build(NavigationBuilder builder) =>
        builder
            .Add(T["Tools"], tools => tools
                .Add(T["Short URLs"], T["Short URLs"].PrefixPosition(), testing => testing
                    .AddClass("shorturls")
                    .Id("shorturls")
                    .Action(
                        nameof(AdminController.List),
                        typeof(AdminController).ControllerName(),
                        new { area = "OrchardCore.Contents", contentTypeId = ContentTypes.ShortUrl })
                    .LocalNav()
                )
            );
}

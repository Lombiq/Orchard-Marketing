using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Lombiq.Marketing.UrlShortener.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents;
using OrchardCore.Contents.Controllers;
using OrchardCore.Contents.Security;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using System.Threading.Tasks;

namespace Lombiq.Marketing.UrlShortener.Navigation;

public sealed class UrlShortenerAdminMenu : AdminMenuNavigationProviderBase
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    public UrlShortenerAdminMenu(
        IHttpContextAccessor hca,
        IStringLocalizer<UrlShortenerAdminMenu> stringLocalizer,
        IContentDefinitionManager contentDefinitionManager)
        : base(hca, stringLocalizer) =>
        _contentDefinitionManager = contentDefinitionManager;

    protected override async Task BuildAsync(NavigationBuilder builder)
    {
        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(ContentTypes.ShortUrl);
        var permission = ContentTypePermissionsHelper.CreateDynamicPermission(
            ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.EditOwnContent.Name],
            contentTypeDefinition);
        builder
            .Add(T["Tools"], tools => tools
                .Add(T["Short URLs"], T["Short URLs"].PrefixPosition(), testing => testing
                    .AddClass("shorturls")
                    .Id("shorturls")
                    .Action(
                        nameof(AdminController.List),
                        typeof(AdminController).ControllerName(),
                        new
                        {
                            area = "OrchardCore.Contents",
                            contentTypeId = ContentTypes.ShortUrl,
                        })
                    .Permission(permission)
                    .LocalNav()
                )
            );
    }
}

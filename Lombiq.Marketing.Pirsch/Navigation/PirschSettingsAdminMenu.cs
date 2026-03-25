using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Lombiq.Marketing.Pirsch.Drivers;
using Lombiq.Marketing.Pirsch.Permissions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace Lombiq.Marketing.Pirsch.Navigation;

public sealed class PirschSettingsAdminMenu : AdminMenuNavigationProviderBase
{
    public PirschSettingsAdminMenu(IHttpContextAccessor hca, IStringLocalizer stringLocalizer)
        : base(hca, stringLocalizer)
    {
    }

    protected override void Build(NavigationBuilder builder) =>
        builder.Add(T["Configuration"], configuration => configuration
            .Add(T["Settings"], settings => settings
                .Add(T["Marketing"], marketing => marketing
                    .Add(T["Pirsch"], T["Pirsch"], pirsch => pirsch
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = PirschSettingsDriver.GroupId })
                        .Permission(PirschSettingsPermissions.ManagePirschSettings)
                        .LocalNav()))));
}

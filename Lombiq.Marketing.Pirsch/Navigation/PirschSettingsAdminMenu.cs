using Lombiq.Marketing.Pirsch.Drivers;
using Lombiq.Marketing.Pirsch.Permissions;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Navigation;

public sealed class PirschSettingsAdminMenu : INavigationProvider
{
    private readonly IStringLocalizer T;

    public PirschSettingsAdminMenu(IStringLocalizer<PirschSettingsAdminMenu> stringLocalizer) => T = stringLocalizer;

    public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!name.EqualsOrdinalIgnoreCase("admin")) return ValueTask.CompletedTask;

        builder.Add(T["Configuration"], configuration => configuration
            .Add(T["Settings"], settings => settings
                .Add(T["Pirsch"], T["Pirsch"], pirsch => pirsch
                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = PirschSettingsDriver.GroupId })
                    .Permission(PirschSettingsPermissions.ManagePirschSettings)
                    .LocalNav())));

        return ValueTask.CompletedTask;
    }
}


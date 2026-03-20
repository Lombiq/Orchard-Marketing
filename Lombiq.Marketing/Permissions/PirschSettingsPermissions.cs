using OrchardCore.Security.Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Permissions;

public sealed class PirschSettingsPermissions : IPermissionProvider
{
    public static readonly Permission ManagePirschSettings = new(
        nameof(ManagePirschSettings),
        "Manage Pirsch settings.");

    public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
        Task.FromResult(new[]
        {
            ManagePirschSettings,
        }.AsEnumerable());

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new()
        {
            Name = "Administrator",
            Permissions = [ManagePirschSettings],
        },
    ];
}

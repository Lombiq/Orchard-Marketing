using Lombiq.Marketing.Drivers;
using Lombiq.Marketing.Pirsch.Models;
using Lombiq.Marketing.Pirsch.Permissions;
using Lombiq.Marketing.Pirsch.Services;
using Lombiq.Marketing.Pirsch.ViewModels;
using Lombiq.Marketing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Drivers;

public sealed class PirschSettingsDriver : ClientSideTrackingSiteDisplayDriver<PirschSettings>
{
    public const string GroupId = nameof(PirschSettings);

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected override string SettingsGroupId => GroupId;

    public PirschSettingsDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IClientSideTrackingMarkupService clientSideTrackingMarkupService)
        : base(clientSideTrackingMarkupService)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<IDisplayResult?> EditDriverAsync(ISite model, PirschSettings section, BuildEditorContext context)
    {
        if (!await IsAuthorizedToManagePirschSettingsAsync()) return null;

        return Initialize<PirschSettingsViewModel>(
            $"{nameof(PirschSettings)}_Edit",
            viewModel =>
            {
                viewModel.ClientSecret = string.Empty;
                viewModel.HasClientSecret = !string.IsNullOrWhiteSpace(section.ClientSecret);
                viewModel.ClearClientSecret = false;
                viewModel.ClientSideCodeSnippet = section.ClientSideCodeSnippet;
                viewModel.DataDev = section.DataDev;
                viewModel.AutoRenderZone = section.AutoRenderZone;
            })
            .Location("Content:1")
            .OnGroup(GroupId);
    }

    protected override async Task UpdateDriverAsync(ISite model, PirschSettings section, UpdateEditorContext context)
    {
        if (!await IsAuthorizedToManagePirschSettingsAsync())
        {
            return;
        }

        var viewModel = await context.CreateModelAsync<PirschSettingsViewModel>(Prefix);
        if (viewModel.ClearClientSecret)
        {
            section.ClientSecret = string.Empty;
        }
        else if (!string.IsNullOrWhiteSpace(viewModel.ClientSecret))
        {
            section.ClientSecret = viewModel.ClientSecret;
        }

        section.ClientSideCodeSnippet = PirschSettingsSanitizer.SanitizeClientSideCodeSnippet(viewModel.ClientSideCodeSnippet);
        section.DataDev = viewModel.DataDev ?? string.Empty;
        section.AutoRenderZone = viewModel.AutoRenderZone ?? string.Empty;
    }

    private async Task<bool> IsAuthorizedToManagePirschSettingsAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        return user != null && await _authorizationService.AuthorizeAsync(user, PirschSettingsPermissions.ManagePirschSettings);
    }
}

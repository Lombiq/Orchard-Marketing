using Lombiq.Marketing.Pirsch.Models;
using Lombiq.Marketing.Pirsch.Permissions;
using Lombiq.Marketing.Pirsch.Services;
using Lombiq.Marketing.Pirsch.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Drivers;

public sealed class PirschSettingsDriver : SiteDisplayDriver<PirschSettings>
{
    public const string GroupId = nameof(PirschSettings);

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPirschClientSideTrackingViewModelService _pirschClientSideTrackingViewModelService;

    protected override string SettingsGroupId => GroupId;

    public PirschSettingsDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IPirschClientSideTrackingViewModelService pirschClientSideTrackingViewModelService)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _pirschClientSideTrackingViewModelService = pirschClientSideTrackingViewModelService;
    }

    public override async Task<IDisplayResult> EditAsync(ISite model, PirschSettings section, BuildEditorContext context)
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
            })
            .Location("Content:1")
            .OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite model, PirschSettings section, UpdateEditorContext context)
    {
        if (context.GroupId == GroupId)
        {
            if (!await IsAuthorizedToManagePirschSettingsAsync())
            {
                return null;
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
            await _pirschClientSideTrackingViewModelService.InvalidateCachedViewModelAsync();
        }

        return await EditAsync(model, section, context);
    }

    private async Task<bool> IsAuthorizedToManagePirschSettingsAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        return user != null && await _authorizationService.AuthorizeAsync(user, PirschSettingsPermissions.ManagePirschSettings);
    }
}

using Lombiq.Marketing.Models;
using Lombiq.Marketing.Permissions;
using Lombiq.Marketing.Services;
using Lombiq.Marketing.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Drivers;

public sealed class PirschSettingsDriver : SiteDisplayDriver<PirschSettings>
{
    public const string GroupId = nameof(PirschSettings);

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected override string SettingsGroupId => GroupId;

    public PirschSettingsDriver(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
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
        }

        return await EditAsync(model, section, context);
    }

    private async Task<bool> IsAuthorizedToManagePirschSettingsAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        return user != null && await _authorizationService.AuthorizeAsync(user, PirschSettingsPermissions.ManagePirschSettings);
    }
}

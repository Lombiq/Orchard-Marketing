using Lombiq.Marketing.Services;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Drivers;

public abstract class ClientSideTrackingSiteDisplayDriver<TSettings> : SiteDisplayDriver<TSettings>
    where TSettings : new()
{
    private readonly IClientSideTrackingViewModelService _clientSideTrackingViewModelService;

    protected ClientSideTrackingSiteDisplayDriver(IClientSideTrackingViewModelService clientSideTrackingViewModelService) =>
        _clientSideTrackingViewModelService = clientSideTrackingViewModelService;

    public sealed override Task<IDisplayResult?> EditAsync(ISite model, TSettings section, BuildEditorContext context) =>
        EditDriverAsync(model, section, context);

    public sealed override async Task<IDisplayResult?> UpdateAsync(ISite model, TSettings section, UpdateEditorContext context)
    {
        if (context.GroupId == SettingsGroupId)
        {
            await UpdateDriverAsync(model, section, context);
            await _clientSideTrackingViewModelService.InvalidateCachedViewModelAsync();
        }

        return await EditDriverAsync(model, section, context);
    }

    protected abstract Task<IDisplayResult?> EditDriverAsync(ISite model, TSettings section, BuildEditorContext context);

    protected abstract Task UpdateDriverAsync(ISite model, TSettings section, UpdateEditorContext context);
}

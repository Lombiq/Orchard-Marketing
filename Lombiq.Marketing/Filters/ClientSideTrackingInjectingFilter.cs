using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using Lombiq.Marketing.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Filters;

public sealed class ClientSideTrackingInjectingFilter : IAsyncResultFilter
{
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IClientSideTrackingMarkupService _clientSideTrackingMarkupService;
    private readonly IShapeFactory _shapeFactory;

    public ClientSideTrackingInjectingFilter(
        ILayoutAccessor layoutAccessor,
        IClientSideTrackingMarkupService clientSideTrackingMarkupService,
        IShapeFactory shapeFactory)
    {
        _layoutAccessor = layoutAccessor;
        _clientSideTrackingMarkupService = clientSideTrackingMarkupService;
        _shapeFactory = shapeFactory;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.IsNotFullViewRenderingOrIsAdmin())
        {
            await next();
            return;
        }

        var viewModels = await _clientSideTrackingMarkupService.GetViewModelsAsync();
        var renderViewModels = viewModels?.Where(viewModel =>
            !string.IsNullOrWhiteSpace(viewModel.Html) &&
            !string.IsNullOrWhiteSpace(viewModel.Zone));

        if (renderViewModels != null)
        {
            foreach (var viewModel in renderViewModels)
            {
                await _layoutAccessor.AddShapeToZoneAsync(
                    viewModel.Zone,
                    await _shapeFactory.CreateAsync("ZoneClientSideTracking", new { ViewModel = viewModel }));
            }
        }

        await next();
    }
}

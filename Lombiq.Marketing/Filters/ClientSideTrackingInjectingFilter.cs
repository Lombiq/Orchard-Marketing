using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Filters;

public sealed class ClientSideTrackingInjectingFilter : IAsyncResultFilter
{
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IShapeFactory _shapeFactory;

    public ClientSideTrackingInjectingFilter(
        ILayoutAccessor layoutAccessor,
        IShapeFactory shapeFactory)
    {
        _layoutAccessor = layoutAccessor;
        _shapeFactory = shapeFactory;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.IsNotFullViewRenderingOrIsAdmin())
        {
            await next();
            return;
        }

        await _layoutAccessor.AddShapeToZoneAsync("LayoutInjection", await _shapeFactory.CreateAsync("ClientSideTracking"));

        await next();
    }
}

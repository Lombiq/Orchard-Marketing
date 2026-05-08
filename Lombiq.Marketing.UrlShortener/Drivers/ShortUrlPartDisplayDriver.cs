using Lombiq.Marketing.UrlShortener.Helpers;
using Lombiq.Marketing.UrlShortener.Models;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;

namespace Lombiq.Marketing.UrlShortener.Drivers;

public sealed class ShortUrlPartDisplayDriver : ContentPartDisplayDriver<ShortUrlPart>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IContentManager _contentManager;

    public ShortUrlPartDisplayDriver(IHttpContextAccessor httpContextAccessor, IContentManager contentManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _contentManager = contentManager;
    }

    public override IDisplayResult Edit(ShortUrlPart part, BuildPartEditorContext context)
        => BuildShortUrlShape("ShortUrlPart_EditShortUrlDisplay", part);

    public override IDisplayResult Display(ShortUrlPart part, BuildPartDisplayContext context)
        => BuildShortUrlShape("ShortUrlPart_SummaryAdmin", part)
            .Location("SummaryAdmin", "Content:10");

    private ShapeResult BuildShortUrlShape(string shapeType, ShortUrlPart part) =>
        Factory(
            shapeType,
            ctx => ctx.ShapeFactory.CreateAsync(shapeType),
            initializeAsync: shape =>
            {
                shape.Properties["DisplayedUrl"] =
                    ShortUrlHelpers.GetFullShortUrl(part.ShortUrl.Text, _httpContextAccessor.HttpContext)?.OriginalString;

                return Task.CompletedTask;
            })
        .RenderWhen(() => _contentManager.HasPublishedVersionAsync(part.ContentItem));
}

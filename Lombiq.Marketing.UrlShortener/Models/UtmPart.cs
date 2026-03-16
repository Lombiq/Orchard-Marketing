using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace Lombiq.Marketing.UrlShortener.Models;

public class UtmPart : ContentPart
{
    // UtmContent can't be named as Content because it hides the Content from ContentElement. So all text fields are
    // named with the Utm prefix to have the same syntax.
    public TextField UtmSource { get; set; } = new();
    public TextField UtmMedium { get; set; } = new();
    public TextField UtmCampaign { get; set; } = new();
    public TextField UtmContent { get; set; } = new();
    public TextField Term { get; set; } = new();
}

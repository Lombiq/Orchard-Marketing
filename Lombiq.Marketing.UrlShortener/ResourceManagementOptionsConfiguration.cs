using Lombiq.Marketing.UrlShortener.Constants;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace Lombiq.Marketing.UrlShortener;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest = new();

    static ResourceManagementOptionsConfiguration() =>
        _manifest
            .DefineScript(ResourceNames.ShortUrlEditorPreview)
            .SetUrl("~/Lombiq.Marketing.UrlShortener/js/short-url-editor-preview.js")
            .SetVersion("1.0.0");

    public void Configure(ResourceManagementOptions options) => options.ResourceManifests.Add(_manifest);
}

using Lombiq.Marketing.UrlShortener.Constants;
using Lombiq.Marketing.UrlShortener.Indexes;
using Lombiq.Marketing.UrlShortener.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Security;
using OrchardCore.Title.Models;
using System.Linq;
using System.Threading.Tasks;
using YesSql.Sql;

namespace Lombiq.Marketing.UrlShortener.Migrations;

public sealed class ShortUrlMigration : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ShortUrlMigration(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(UtmPart), part => part
            .WithField(nameof(UtmPart.UtmSource), field => field
                .OfType(nameof(TextField))
                .WithDisplayName("UTM Source")
                .WithSettings(new TextFieldSettings
                {
                    Hint = "The UTM source. It is used to identify the source of the traffic (for example: google, newsletter, etc.).",
                }))
            .WithField(nameof(UtmPart.UtmMedium), field => field
                .OfType(nameof(TextField))
                .WithDisplayName("UTM Medium")
                .WithSettings(new TextFieldSettings
                {
                    Hint = "The UTM medium. It is used to identify the medium of the traffic (for example: cpc, email, etc.).",
                }))
            .WithField(nameof(UtmPart.UtmCampaign), field => field
                .OfType(nameof(TextField))
                .WithDisplayName("UTM Campaign")
                .WithSettings(new TextFieldSettings
                {
                    Hint = "The UTM campaign. It is used to identify the campaign of the traffic (for example: summer-sale, etc.).",
                }))
            .WithField(nameof(UtmPart.UtmContent), field => field
                .OfType(nameof(TextField))
                .WithDisplayName("UTM Content")
                .WithSettings(new TextFieldSettings
                {
                    Hint = "The UTM content. It is used to identify the content of the traffic (for example: banner, link, etc.).",
                }))
            .WithField(nameof(UtmPart.UtmTerm), field => field
                .OfType(nameof(TextField))
                .WithDisplayName("UTM Term")
                .WithSettings(new TextFieldSettings
                {
                    Hint = "The UTM term. Only relevant for paid ads and is used to filter for search terms used by visitors." +
                        " It is used to identify the term of the traffic (for example: shoes, etc.).",
                }))
        );

        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(ShortUrlPart), part => part
            .WithField(nameof(ShortUrlPart.ShortUrl), field => field
                .OfType(nameof(TextField))
                .WithDisplayName("Short URL")
                .WithSettings(new TextFieldSettings
                {
                    Required = true,
                    Hint = "The short URL. It can only be a unique relative URL (for example: /short-url).",
                }))
            .WithField(nameof(ShortUrlPart.DestinationUrl), field => field
                .OfType(nameof(TextField))
                .WithDisplayName("Destination URL")
                .WithSettings(new TextFieldSettings
                {
                    Required = true,
                    Hint = "The destination URL. It can be an absolute URL (https://example.com) or a relative URL (/my-page).",
                })));

        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.ShortUrl, type => type
            .DisplayedAs("Short URL")
            .Securable()
            .Creatable()
            .Listable()
            .WithPart<TitlePart>()
            .WithPart<ShortUrlPart>()
            .WithPart<UtmPart>());

        await SchemaBuilder.CreateMapIndexTableAsync<ShortUrlPartIndex>(table => table
            .Column<string>(nameof(ShortUrlPartIndex.ShortUrl))
            .Column<string>(nameof(ShortUrlPartIndex.DestinationUrl))
            .Column<string>(nameof(ShortUrlPartIndex.ContentItemId), column => column.WithLength(26))
        );

        await SchemaBuilder.AlterIndexTableAsync<ShortUrlPartIndex>(table => table
            .CreateIndex($"IDX_{nameof(ShortUrlPartIndex)}_{nameof(ShortUrlPartIndex.ShortUrl)}", nameof(ShortUrlPartIndex.ShortUrl))
        );

        // Simply in the deferred task it still won't find the editor role on setup, because it's not initiated yet.
        // So we execute the task after the end of the request, which is after the setup is completed, so the editor
        // role will be there.
        ShellScope.AddDeferredTask(_ => HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(
            "RemoveDeletePermission",
            async subScope =>
            {
                var roleManager = subScope.ServiceProvider.GetRequiredService<RoleManager<IRole>>();

                var editorRole = await roleManager.FindByNameAsync("Editor");
                var deleteContentClaim = ((Role)editorRole)?
                    .RoleClaims
                    .Where(claim => claim.ClaimValue == CommonPermissions.DeleteContent.Name)
                    .ToArray();

                if (deleteContentClaim == null) return;

                foreach (var claim in deleteContentClaim)
                {
                    await roleManager.RemoveClaimAsync(editorRole, claim);
                }

                await roleManager.UpdateAsync(editorRole);
            }));

        return 1;
    }
}

using Lombiq.Marketing.UrlShortener.Constants;
using Lombiq.Marketing.UrlShortener.Indexes;
using Lombiq.Marketing.UrlShortener.Models;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Title.Models;
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
        await _contentDefinitionManager.AlterPartDefinitionAsync<UtmPart>(part => part
            .WithField(part => part.UtmSource, field => field
                .WithDisplayName("UTM Source")
                .WithSettings(new TextFieldSettings
                {
                    Hint = "The name of the website, social media platform, newsletter, or other campaign source " +
                        "where the link is shared. E.g.: lombiq-newsletter, linkedin, youtube, x, partner-site, google.",
                }))
            .WithField(part => part.UtmMedium, field => field
                .WithDisplayName("UTM Medium")
                .WithSettings(new TextFieldSettings
                {
                    Hint = "The type of the channel where the link is shared. E.g.: social, email, content (when " +
                        "e.g. in a blog post), cpc, cta (for CTA widgets), partner (for partner sites).",
                }))
            .WithField(part => part.UtmCampaign, field => field
                .WithDisplayName("UTM Campaign")
                .WithSettings(new TextFieldSettings
                {
                    Hint = "The name of the specific campaign, identifying all links shared during the campaign. " +
                        "E.g.: summer-sale-2026, lombiq-newsletter-2026-03-31.",
                }))
            .WithField(part => part.UtmContent, field => field
                .WithDisplayName("UTM Content")
                .WithSettings(new TextFieldSettings
                {
                    Hint = "The name of the specific link. Useful if you have multiple links from e.g. the same " +
                        "landing page or blog post. E.g.: banner, cta-button, foot-menu-link.",
                }))
            .WithField(part => part.UtmTerm, field => field
                .WithDisplayName("UTM Term")
                .WithSettings(new TextFieldSettings
                {
                    Hint = "Only relevant for paid search ads and is used to filter for search terms (keywords) used " +
                        "by visitors.",
                }))
        );

        await _contentDefinitionManager.AlterPartDefinitionAsync<ShortUrlPart>(part => part
            .WithField(part => part.ShortUrl, field => field
                .WithDisplayName("Short URL")
                .WithSettings(new TextFieldSettings
                {
                    Required = true,
                    Hint = "The short URL can only be a unique relative URL (for example: /jmp/short-url). /jmp prefix " +
                        "will be automatically added if not included.",
                }))
            .WithField(part => part.DestinationUrl, field => field
                .WithDisplayName("Destination URL")
                .WithSettings(new TextFieldSettings
                {
                    Required = true,
                    Hint = "The destination URL can be an absolute URL (https://example.com) or a relative URL (/my-page).",
                })));

        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.ShortUrl, type => type
            .WithDisplayName("Short URL")
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

        return 1;
    }
}

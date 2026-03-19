using Lombiq.Marketing.UrlShortener.Constants;
using Lombiq.Marketing.UrlShortener.Indexes;
using Lombiq.Marketing.UrlShortener.Models;
using OrchardCore.ContentFields.Fields;
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
            .WithField(nameof(UtmPart.Term), field => field
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

        return 1;
    }
}

using Atata;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Helpers;
using Lombiq.Tests.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using OpenQA.Selenium;
using Shouldly;
using System;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lombiq.Marketing.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    private const string TestPirschScript = "<script defer=\"\" id=\"pianjs\" data-code=\"test\" data-dev=\"test\"></script>";
    private const string UpdatedTestPirschScript = "<script defer=\"\" id=\"pianjsUpdated\" data-code=\"test\"></script>";
    private const string UpdatedTestPirschScriptWithOverrideDataDev =
        "<script defer=\"\" id=\"pianjsUpdated\" data-code=\"test\" data-dev=\"open-source-orchard-core-extensions.com\"></script>";

    public static async Task TestShortUrlManagementAsync(this UITestContext context)
    {
        const string title = "Marketing Short URL Test";
        const string shortUrl = "/marketing-short-url";
        const string updatedShortUrl = "/marketing-short-url-updated";

        await context.SignInDirectlyAndGoToDashboardAsync();

        await context.ClickReliablyOnByLinkTextAsync("Tools");
        await context.ClickReliablyOnByLinkTextAsync("Short URLs");
        await context.ClickReliablyOnByLinkTextAsync("New Short URL");

        await context.FillContentItemTitleAsync(title);
        await context.FillShortUrlFieldAsync("ShortUrlPart_ShortUrl_Text", shortUrl);
        await context.FillShortUrlFieldAsync("ShortUrlPart_DestinationUrl_Text", "/");

        await context.ClickReliablyOnByLinkTextAsync("UTM Parameters");
        await context.FillShortUrlFieldAsync("UtmPart_UtmSource_Text", "newsletter");
        await context.FillShortUrlFieldAsync("UtmPart_UtmMedium_Text", "email");
        await context.FillShortUrlFieldAsync("UtmPart_UtmCampaign_Text", "spring-sale");
        await context.FillShortUrlFieldAsync("UtmPart_UtmContent_Text", "hero-banner");
        await context.FillShortUrlFieldAsync("UtmPart_UtmTerm_Text", "orchard-core");

        await context.ClickPublishAsync();
        context.ShouldBeSuccess();

        await context.GoToRelativeUrlAsync(shortUrl, onlyIfNotAlreadyThere: false);
        var redirectedUri = context.GetCurrentUri();
        redirectedUri.AbsolutePath.ShouldBe("/");
        QueryHelpers.ParseQuery(redirectedUri.Query).ShouldBeEmpty();

        await context.GoToDashboardAsync();
        await context.ClickReliablyOnByLinkTextAsync("Short URLs");

        await context.FilterOnAdminAsync(title);
        context.Exists(By.XPath($"//a[normalize-space()='{title}']")).ShouldBeTrue();

        await context.ClickReliablyOnAsync(By.XPath($"//a[normalize-space()='{title}']"));
        await context.FillShortUrlFieldAsync("ShortUrlPart_ShortUrl_Text", updatedShortUrl);
        await context.ClickReliablyOnByLinkTextAsync("UTM Parameters");
        await context.FillShortUrlFieldAsync("UtmPart_UtmCampaign_Text", "summer-sale");
        await context.ClickPublishAsync();
        context.ShouldBeSuccess();

        await context.GoToRelativeUrlAsync(updatedShortUrl, onlyIfNotAlreadyThere: false);
        var updatedUri = context.GetCurrentUri();
        updatedUri.AbsolutePath.ShouldBe("/");
        QueryHelpers.ParseQuery(updatedUri.Query).ShouldBeEmpty();

        await context.GoToContentItemListAsync("ShortUrl");
        await context.FilterOnAdminAsync(title);
        await context.ClickReliablyOnAsync(By.XPath("//button[contains(.,'Actions')]"));
        await context.ClickReliablyOnByLinkTextAsync("Delete");
        await context.ClickModalOkAsync();
        context.ShouldBeSuccess();

        await context.GoToContentItemListAsync("ShortUrl");
        await context.FilterOnAdminAsync(title);
        context.Exists(By.XPath($"//a[normalize-space()='{title}']").Safely()).ShouldBeFalse();

        await context.GoToRelativeUrlAsync(updatedShortUrl, onlyIfNotAlreadyThere: false);
        context.GetCurrentUri().AbsolutePath.ShouldBe(updatedShortUrl);
        await context.GoToRelativeUrlAsync(shortUrl, onlyIfNotAlreadyThere: false);
        context.GetCurrentUri().AbsolutePath.ShouldBe(shortUrl);
    }

    public static async Task TestPirschClientSideTrackingAutomaticInjectionAsync(this UITestContext context)
    {
        var pageSource = context.Driver.PageSource;
        pageSource.ContainsOrdinalIgnoreCase(TestPirschScript).ShouldBeTrue();

        await context.SignInDirectlyAndGoToDashboardAsync();

        await context.ClickReliablyOnByLinkTextAsync("Configuration");
        await context.ClickReliablyOnByLinkTextAsync("Settings");
        await context.ClickReliablyOnByLinkTextAsync("Marketing");
        await context.ClickReliablyOnByLinkTextAsync("Pirsch");

        await context.ClickAndFillInWithRetriesAsync(By.Id("ISite_PirschSettings_ClientSideCodeSnippet"), UpdatedTestPirschScript);
        await context.ClickReliablyOnSubmitAsync();
        context.ShouldBeSuccess();

        await context.GoToHomePageAsync();

        context.Driver.PageSource.ContainsOrdinalIgnoreCase(UpdatedTestPirschScriptWithOverrideDataDev).ShouldBeTrue();

        await context.GoToAdminRelativeUrlAsync("/Settings/PirschSettings");
        await context.ClickAndFillInWithRetriesAsync(By.Id("ISite_PirschSettings_DataDev"), "newDataDev");
        await context.ClickReliablyOnSubmitAsync();
        context.ShouldBeSuccess();

        await context.GoToHomePageAsync();

        context.Driver.PageSource.ContainsOrdinalIgnoreCase(UpdatedTestPirschScript).ShouldBeTrue();
        context.Driver.PageSource.ContainsOrdinalIgnoreCase("data-dev=\"newDataDev\"").ShouldBeTrue();
    }

    public static void SetPirschClientTrackerConfiguration(this OrchardCoreUITestExecutorConfiguration configuration) =>
        configuration.OrchardCoreConfiguration.BeforeAppStart +=
            (_, argumentsBuilder) =>
            {
                argumentsBuilder
                    .AddWithValue(
                        "OrchardCore:Lombiq_Marketing:Pirsch:ClientSideCodeSnippet",
                        TestPirschScript);

                return Task.CompletedTask;
            };

    public static void SetShortUrlConfiguration(this OrchardCoreUITestExecutorConfiguration configuration)
    {
        configuration.AssertAppLogsAsync = app =>
            app.LogsShouldNotContainAsync(logEntry => IsUnexpectedAppLog(logEntry), configuration.TestCancellationToken);

        configuration.ResponseLogFilter = e => e.IsNonSuccessResponseAndNotExpectedStatusResponse("/marketing-short-url", 404);
    }

    private static bool IsUnexpectedAppLog(IApplicationLogEntry logEntry) =>
        AppLogAssertionHelper.NotMediaCacheEntries(logEntry) &&
        logEntry.Level >= LogLevel.Error &&
        !IsExpectedPirschShortUrlError(logEntry);

    private static bool IsExpectedPirschShortUrlError(IApplicationLogEntry logEntry) =>
        (logEntry.Category == "Lombiq.Marketing.Pirsch.Services.PirschApiClient" &&
        logEntry.Message.ContainsOrdinalIgnoreCase("Cannot send a request to Pirsch API because the client secret is not configured")) ||
        (logEntry.Category == "Lombiq.Marketing.Pirsch.Services.PirschShortUrlHitHandler" &&
        logEntry.Message.ContainsOrdinalIgnoreCase("Failed to send a hit to Pirsch API for the tracked target URL:"));

    public static Task FillShortUrlFieldAsync(this UITestContext context, string id, string value) =>
        context.ClickAndFillInWithRetriesAsync(
            By.Id(id),
            value);
}

using Lombiq.Tests.UI.Extensions;
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
    private const string ShortUrl = "/jmp/marketing-short-url";
    private const string ShortUrlUpdated = $"{ShortUrl}-updated";
    private const string TestPirschScript = "<script defer=\"\" id=\"pianjs\" data-code=\"test\" data-dev=\"test\"></script>";

    public static async Task TestShortUrlManagementAsync(this UITestContext context)
    {
        async Task ShortUrlShouldBeInactiveAsync(string url)
        {
            await context.GoToRelativeUrlAsync(url, onlyIfNotAlreadyThere: false);
            context.GetCurrentUri().AbsolutePath.ShouldBe(url);
            context.Exists(By.XPath("//h1[contains(., 'The page could not be found.')]"));
        }

        const string title = "Marketing Short URL Test";

        await context.SignInDirectlyAndGoToDashboardAsync();

        await context.ClickReliablyOnByLinkTextAsync("Tools");
        await context.ClickReliablyOnByLinkTextAsync("Short URLs");
        await context.ClickReliablyOnByLinkTextAsync("New Short URL");

        await context.FillContentItemTitleAsync(title);
        await context.FillShortUrlFieldAsync("ShortUrlPart_ShortUrl_Text", ShortUrl);
        await context.FillShortUrlFieldAsync("ShortUrlPart_DestinationUrl_Text", "/");

        await context.ClickReliablyOnByLinkTextAsync("UTM Parameters");
        await context.FillShortUrlFieldAsync("UtmPart_UtmSource_Text", "newsletter");
        await context.FillShortUrlFieldAsync("UtmPart_UtmMedium_Text", "email");
        await context.FillShortUrlFieldAsync("UtmPart_UtmCampaign_Text", "spring-sale");
        await context.FillShortUrlFieldAsync("UtmPart_UtmContent_Text", "hero-banner");
        await context.FillShortUrlFieldAsync("UtmPart_UtmTerm_Text", "orchard-core");

        await context.ClickPublishAsync();
        context.ShouldBeSuccess();

        await context.GoToRelativeUrlAsync(ShortUrl, onlyIfNotAlreadyThere: false);
        var redirectedUri = context.GetCurrentUri();
        redirectedUri.AbsolutePath.ShouldBe("/");
        QueryHelpers.ParseQuery(redirectedUri.Query).ShouldBeEmpty();

        await context.GoToDashboardAsync();
        await context.ClickReliablyOnByLinkTextAsync("Short URLs");

        await context.FilterOnAdminAsync(title);
        context.Exists(By.XPath($"//a[normalize-space()='{title}']"));

        await context.ClickReliablyOnAsync(By.XPath($"//a[normalize-space()='{title}']"));
        await context.FillShortUrlFieldAsync("ShortUrlPart_ShortUrl_Text", ShortUrlUpdated);
        await context.ClickReliablyOnByLinkTextAsync("UTM Parameters");
        await context.FillShortUrlFieldAsync("UtmPart_UtmCampaign_Text", "summer-sale");
        await context.ClickPublishAsync();
        context.ShouldBeSuccess();

        await context.GoToRelativeUrlAsync(ShortUrlUpdated, onlyIfNotAlreadyThere: false);
        var updatedUri = context.GetCurrentUri();
        updatedUri.AbsolutePath.ShouldBe("/");
        QueryHelpers.ParseQuery(updatedUri.Query).ShouldBeEmpty();

        await ShortUrlShouldBeInactiveAsync(ShortUrl);

        await context.GoToContentItemListAsync("ShortUrl");
        await context.FilterOnAdminAsync(title);
        await context.ClickReliablyOnAsync(By.XPath("//button[contains(.,'Actions')]"));
        await context.ClickReliablyOnByLinkTextAsync("Delete");
        await context.ClickModalOkAsync();
        context.ShouldBeSuccess();

        await context.GoToContentItemListAsync("ShortUrl");
        await context.FilterOnAdminAsync(title);
        context.Missing(By.XPath($"//a[normalize-space()='{title}']"));

        await ShortUrlShouldBeInactiveAsync(ShortUrlUpdated);
        await ShortUrlShouldBeInactiveAsync(ShortUrl);
    }

    public static async Task TestPirschClientSideTrackingAutomaticInjectionAsync(this UITestContext context)
    {
        AssertPirschSnippet(context.Driver.PageSource, "pianjs", "test");

        await context.SignInDirectlyAndGoToDashboardAsync();

        await context.ClickReliablyOnByLinkTextAsync("Settings");
        await context.ClickReliablyOnByLinkTextAsync("Marketing");
        await context.ClickReliablyOnUntilNavigationHasOccurredAsync(By.LinkText("Pirsch"));
        context.GetCurrentUri().AbsolutePath.ShouldEndWith("/Settings/PirschSettings");

        await context.ClickAndFillInWithRetriesAsync(
            By.Id("ISite_PirschSettings_ClientSideCodeSnippet"),
            "<script defer=\"\" id=\"pianjs\" data-code=\"test\"></script>");
        await context.ClickReliablyOnSubmitAsync();
        context.ShouldBeSuccess();

        await context.GoToHomePageAsync();

        AssertPirschSnippet(context.Driver.PageSource, "pianjs", "open-source-orchard-core-extensions.com");

        await context.GoToAdminRelativeUrlAsync("/Settings/PirschSettings");
        await context.ClickAndFillInWithRetriesAsync(By.Id("ISite_PirschSettings_DataDev"), "newDataDev");
        await context.ClickReliablyOnSubmitAsync();
        context.ShouldBeSuccess();

        await context.GoToHomePageAsync();

        AssertPirschSnippet(context.Driver.PageSource, "pianjs", "newDataDev");
    }

    public static void SetPirschClientTrackerConfiguration(this OrchardCoreUITestExecutorConfiguration configuration)
    {
        configuration.AssertAppLogsAsync = app =>
            app.LogsShouldNotContainAsync(logEntry => IsUnexpectedAppLog(logEntry), configuration.TestCancellationToken);

        configuration.WithIgnoreExpectedNotFoundResponseFilter("/secret-sauce/pv");
        configuration.OrchardCoreConfiguration.BeforeAppStart +=
            (_, argumentsBuilder) =>
            {
                argumentsBuilder
                    .AddWithValue(
                        "OrchardCore:Lombiq_Marketing:Pirsch:ClientSideCodeSnippet",
                        TestPirschScript);

                return Task.CompletedTask;
            };
    }

    public static void SetShortUrlConfiguration(this OrchardCoreUITestExecutorConfiguration configuration)
    {
        configuration.AssertAppLogsAsync = app =>
            app.LogsShouldNotContainAsync(logEntry => IsUnexpectedAppLog(logEntry), configuration.TestCancellationToken);

        configuration.WithIgnoreExpectedNotFoundResponseFilter(ShortUrl);
        configuration.WithIgnoreExpectedNotFoundResponseFilter(ShortUrlUpdated);
    }

    private static bool IsUnexpectedAppLog(IApplicationLogEntry logEntry) =>
        logEntry.Level >= LogLevel.Error &&
        !IsExpectedPirschShortUrlError(logEntry);

    // Since we don't want to send real data, we have to suppress these.
    private static bool IsExpectedPirschShortUrlError(IApplicationLogEntry logEntry) =>
        (logEntry.Category == "Lombiq.Marketing.Pirsch.Services.PirschApiClient" &&
        logEntry.Message.ContainsOrdinalIgnoreCase("Cannot send a request to Pirsch API because the client secret is not configured")) ||
        (logEntry.Category == "Lombiq.Marketing.Pirsch.Services.PirschShortUrlHitHandler" &&
        logEntry.Message.ContainsOrdinalIgnoreCase("Failed to send a hit to Pirsch API for the tracked target URL:"));

    public static Task FillShortUrlFieldAsync(this UITestContext context, string id, string value) =>
        context.ClickAndFillInWithRetriesAsync(
            By.Id(id),
            value);

    private static void AssertPirschSnippet(
        string pageSource,
        string id,
        string dataDev)
    {
        pageSource.ShouldContain($"id=\"{id}\"");
        pageSource.ShouldContain("src=\"/secret-sauce/sauce.js\"");
        pageSource.ShouldContain("data-hit-endpoint=\"/secret-sauce/pv\"");
        pageSource.ShouldContain("data-event-endpoint=\"/secret-sauce/e\"");
        pageSource.ShouldContain("data-session-endpoint=\"/secret-sauce/s\"");
        pageSource.ShouldContain("data-code=\"test\"");
        pageSource.ShouldContain($"data-dev=\"{dataDev}\"");
    }
}

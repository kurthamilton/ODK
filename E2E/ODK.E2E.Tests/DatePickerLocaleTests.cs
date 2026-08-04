using System.Globalization;
using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// The date picker displays dates in the request's locale while still posting a fixed format. The locale is
/// resolved per request from the Accept-Language header (there's no stored preference), so each test drives a
/// browser context with a specific <see cref="BrowserNewContextOptions.Locale"/> - Playwright sends that as
/// the Accept-Language header. The root layout emits the resolved short-date format and the shared
/// date-picker JS applies it as flatpickr's altFormat. Platform-agnostic, so this runs on Default only.
/// </summary>
[TestFixture]
public class DatePickerLocaleTests : DefaultPageTest
{
    protected override string PlatformBaseUrl => E2ESettings.DefaultBaseUrl;

    [Test]
    public async Task DatePicker_WithGbRequestLocale_DisplaysUkFormat()
    {
        // Act - read the picker on a request whose Accept-Language is en-GB.
        var (value, display) = await ReadPickerWithLocale("en-GB");

        // Assert - the en-GB display matches the posted UK format exactly.
        value.Should().Be("05/08/2026 19:00");
        display.Should().Be("05/08/2026 19:00");
    }

    [Test]
    public async Task DatePicker_WithUsRequestLocale_DisplaysUsFormat()
    {
        // Act - read the picker on a request whose Accept-Language is en-US.
        var (value, display) = await ReadPickerWithLocale("en-US");

        // Assert - the posted value stays UK-format; the visible display is the same instant, US-ordered.
        value.Should().Be("05/08/2026 19:00");
        display.Should().NotBe(value);
        DateTime.ParseExact(display, "M/d/yyyy HH:mm", CultureInfo.InvariantCulture)
            .Should().Be(new DateTime(2026, 8, 5, 19, 0, 0));
    }

    [Test]
    public async Task RenderedEventDate_FollowsRequestLocale()
    {
        // Arrange - a published event 14 days out. Its admin header renders the date through the friendly
        // formatter, which follows CultureInfo.CurrentCulture - set per request from the Accept-Language
        // header by the request-localisation middleware. (This is a different path from the date picker,
        // which reads HttpRequestContext.Locale.)
        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreatePublishedGroup(owner, $"e2elocale{Guid.NewGuid():N}");
        var routes = PlatformRoutes.Default(group);
        var @event = await Provisioning.CreatePublishedEvent(owner, routes, group.ChapterId, PlatformBaseUrl);

        // The event is created 14 days out at 19:00 (see Provisioning); assert on the day + month tokens,
        // which are present whether or not the friendly format also shows the year.
        var eventDate = DateTime.Today.AddDays(14);
        var day = eventDate.Day;
        var month = eventDate.ToString("MMM", CultureInfo.GetCultureInfo("en-GB"));

        // Act - read the same event-admin page under each request locale.
        var editUrl = routes.EventEdit(@event.EventId);
        var gb = await ReadEventAdminBodyWithLocale("en-GB", owner, editUrl);
        var us = await ReadEventAdminBodyWithLocale("en-US", owner, editUrl);

        // Assert - en-GB writes the day before the month, en-US the month before the day.
        gb.Should().Contain($"{day} {month}").And.NotContain($"{month} {day}");
        us.Should().Contain($"{month} {day}").And.NotContain($"{day} {month}");
    }

    // Logs the owner in on a browser context whose locale sets the Accept-Language header, opens the given
    // admin page, and returns its rendered body text.
    private async Task<string> ReadEventAdminBodyWithLocale(string locale, TestAccount owner, string adminUrl)
    {
        await using var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = PlatformBaseUrl,
            Locale = locale
        });
        var page = await context.NewPageAsync();

        await new LoginPage(page).LogIn(owner.Email, owner.Password);
        await page.Navigate(adminUrl);
        return await page.InnerTextAsync("body");
    }

    // Provisions a fresh owner + published group + venue (the create-event Date field only renders once the
    // chapter has a venue), then reads the create-event date picker on a browser context whose locale sets
    // the Accept-Language header. A fresh owner per test keeps the parallel fixtures independent.
    private async Task<(string Value, string Display)> ReadPickerWithLocale(string locale)
    {
        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreatePublishedGroup(owner, $"e2elocale{Guid.NewGuid():N}");
        var routes = PlatformRoutes.Default(group);

        await using var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = PlatformBaseUrl,
            Locale = locale
        });
        var page = await context.NewPageAsync();

        await new LoginPage(page).LogIn(owner.Email, owner.Password);
        await new VenueAdminPage(page).CreateVenue(routes.VenueCreate, $"E2E Venue {Guid.NewGuid():N}");
        return await new EventAdminPage(page).SetDateAndReadPicker(routes.EventCreate, "05/08/2026 19:00");
    }
}

using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// The date picker displays dates in the viewer's resolved locale while still posting a fixed format.
/// The mechanism (the root layout emits the format; the shared date-picker JS applies it as flatpickr's
/// altFormat) is platform-agnostic, so this runs on the Default platform only. The owner's locale is
/// seeded directly because there's no settings UI yet.
/// </summary>
[TestFixture]
public class DatePickerLocaleTests : DefaultPageTest
{
    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberPreferencesDataHelper Preferences => new(E2ESettings.ConnectionString);

    protected override string PlatformBaseUrl => E2ESettings.DefaultBaseUrl;

    [Test]
    public async Task DatePicker_WithNoLocalePreference_DisplaysDefaultFormat()
    {
        // Arrange - an owner with no locale preference and no location, so the app falls back to en-GB.
        var (routes, _) = await ProvisionOwnerWithVenue();

        // Act - set a date and read both the posted value and the visible display.
        var (value, display) = await new EventAdminPage(Page)
            .SetDateAndReadPicker(routes.EventCreate, "05/08/2026 19:00");

        // Assert - the default (en-GB) display matches the posted UK format exactly.
        value.Should().Be("05/08/2026 19:00");
        display.Should().Be("05/08/2026 19:00");
    }

    [Test]
    public async Task DatePicker_WithUsLocalePreference_DisplaysDateInUsFormat()
    {
        // Arrange - an owner whose locale preference is US.
        var (routes, owner) = await ProvisionOwnerWithVenue(locale: "en-US");

        // Act - set a date (5 August, where day/month order is visible) and read both values.
        var (value, display) = await new EventAdminPage(Page)
            .SetDateAndReadPicker(routes.EventCreate, "05/08/2026 19:00");

        // Assert - the posted value stays UK-format; the visible display is the same instant, US-ordered.
        value.Should().Be("05/08/2026 19:00");
        display.Should().NotBe(value);
        DateTime.ParseExact(display, "M/d/yyyy HH:mm", CultureInfo.InvariantCulture)
            .Should().Be(new DateTime(2026, 8, 5, 19, 0, 0));
    }

    // A fresh owner + published chapter + venue (needed for the create-event Date field to render), logged
    // in and ready to drive the create-event form. A fresh owner is used because seeding a locale mutates
    // the member, so it can't be shared.
    private async Task<(PlatformRoutes Routes, TestAccount Owner)> ProvisionOwnerWithVenue(string? locale = null)
    {
        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreatePublishedGroup(owner, $"e2elocale{Guid.NewGuid():N}");
        var routes = PlatformRoutes.Default(group);

        if (locale != null)
        {
            var ownerId = await Members.GetMemberId(owner.Email);
            await Preferences.SetLocale(ownerId, locale);
        }

        await new LoginPage(Page).LogIn(owner.Email, owner.Password);
        await new VenueAdminPage(Page).CreateVenue(routes.VenueCreate, $"E2E Venue {Guid.NewGuid():N}");

        return (routes, owner);
    }
}

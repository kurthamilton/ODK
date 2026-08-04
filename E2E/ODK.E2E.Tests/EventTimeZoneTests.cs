using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// An event's start/end time stays in the chapter (venue) timezone; when the viewing member is in a
/// different timezone the display adds a UTC-offset indicator (e.g. "(UTC+1)") so the wall-clock time
/// isn't misread as the member's own local time. This drives that end to end - a member in a far-away
/// timezone opens an event and sees the chapter-zone time plus the offset label. Platform-agnostic, so
/// it runs on Default only.
/// </summary>
[TestFixture]
public class EventTimeZoneTests : DefaultPageTest
{
    protected override string PlatformBaseUrl => E2ESettings.DefaultBaseUrl;

    private static ChapterDataHelper Chapters => new(E2ESettings.ConnectionString);

    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    [Test]
    public async Task EventTime_MemberInDifferentTimeZone_ShowsChapterZoneTimeWithOffsetLabel()
    {
        // Arrange - a published, timed (19:00) event in a chapter, and a member of that chapter whose own
        // timezone is far away (Sydney). The member's timezone is set directly because the app only derives
        // it by geocoding a location, which isn't wired up in the e2e environment.
        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreatePublishedGroup(owner, $"e2etz{Guid.NewGuid():N}");
        var routes = PlatformRoutes.Default(group);
        var @event = await Provisioning.CreatePublishedEvent(owner, routes, group.ChapterId, PlatformBaseUrl);

        var member = await Provisioning.JoinGroupAsMember(group);
        await Members.SetTimeZone(await Members.GetMemberId(member.Email), "AUS Eastern Standard Time");

        // The event is created 14 days out at 19:00 chapter-local (see Provisioning). Compute the chapter
        // zone's offset at that local time with the framework - the e2e solution can't reference app code.
        var chapterZone = TimeZoneInfo.FindSystemTimeZoneById(await Chapters.GetTimeZoneId(group.ChapterId));
        var offset = chapterZone.GetUtcOffset(DateTime.Today.AddDays(14).AddHours(19));
        var magnitude = offset.Duration();
        var offsetText = magnitude.Minutes == 0 ? $"{magnitude.Hours}" : $"{magnitude.Hours}:{magnitude.Minutes:D2}";
        var expectedLabel = $"(UTC{(offset < TimeSpan.Zero ? "-" : "+")}{offsetText})";

        // Act - the member opens the event page.
        await new LoginPage(Page).LogIn(member.Email, member.Password);
        await Page.Navigate(routes.EventPage(@event.Shortcode));
        var body = await Page.InnerTextAsync("body");

        // Assert - the event keeps its chapter-zone wall-clock time (19:00, not converted to Sydney) and
        // carries the chapter zone's UTC-offset indicator for the differently-zoned viewer.
        body.Should().Contain("19:00");
        body.Should().Contain(expectedLabel);
    }
}

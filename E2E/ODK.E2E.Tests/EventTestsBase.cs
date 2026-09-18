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
/// Event-admin and RSVP scenarios, written once and run against both platforms. The same admin function
/// lives under different route trees and chrome per platform, but the forms are identical - so the
/// scenario bodies here are shared, and the concrete per-platform fixtures
/// (<see cref="EventTests"/> / <see cref="DrunkenKnitwitsEventTests"/>) supply only the platform base
/// URL + category and the platform-specific provisioning (owner+chapter, member) and route
/// building.
/// </summary>
public abstract class EventTestsBase : OdkPageTest
{
    // EventResponseType.Yes - referenced as a literal because the E2E solution deliberately doesn't
    // reference the app's assemblies.
    private const int ResponseYes = 1;

    private static ChapterEventSettingsDataHelper EventSettingsData => new(E2ESettings.ConnectionString);

    private static EventDataHelper Events => new(E2ESettings.ConnectionString);

    private static EventResponseDataHelper EventResponses => new(E2ESettings.ConnectionString);

    private static VenueDataHelper Venues => new(E2ESettings.ConnectionString);

    [Test]
    public async Task CreateEvent_RequiredFieldsOnly_CreatesEvent()
    {
        // Arrange - an owner with a published chapter, and a venue to hold the event (Venue is required).
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        var venueName = $"E2E Venue {Guid.NewGuid():N}";
        await new VenueAdminPage(Page).CreateVenue(routes.VenueCreate, venueName, E2ESettings.VenueExternalId(0));
        var chapterVenueId = await Venues.GetChapterVenueId(group.ChapterId, venueName);
        chapterVenueId.Should().NotBeNull();

        // Act - create an event setting only the required fields (Name, Venue, Date).
        var eventName = $"E2E Event {Guid.NewGuid():N}";
        var date = $"{DateTime.Today.AddDays(14):dd/MM/yyyy} 19:00";
        await new EventAdminPage(Page).CreateEvent(routes.EventCreate, eventName, chapterVenueId!.Value, date);

        // Assert - the event was created for the chapter.
        var eventId = await Events.GetEventId(group.ChapterId, eventName);
        eventId.Should().NotBeNull();
    }

    [Test]
    public async Task CreateEvent_WithDefaultDayAndTime_PrepopulatesDate()
    {
        // Arrange - an owner with a published chapter, and a default event day/time configured.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        var defaultDay = DayOfWeek.Wednesday;
        await new EventSettingsPage(Page).SetDefaults(routes.EventSettings, defaultDay, "19:00");

        // The create-event form (and its Date field) only renders once the chapter has a venue.
        await new VenueAdminPage(Page).CreateVenue(routes.VenueCreate, $"E2E Venue {Guid.NewGuid():N}", E2ESettings.VenueExternalId(1));

        // Act - open the create-event page; its Date defaults to the next default day at the default time.
        var dateValue = await new EventAdminPage(Page).GetPrepopulatedDate(routes.EventCreate);

        // Assert - the pre-populated Date is the next instance of the default day at the default time.
        dateValue.Should().NotBeNullOrWhiteSpace();
        var date = DateTime.ParseExact(dateValue, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        date.DayOfWeek.Should().Be(defaultDay);
        date.TimeOfDay.Should().Be(new TimeSpan(19, 0, 0));
        // "next" instance: a future date within a week (a day of slack absorbs the chapter/runner
        // timezone difference).
        date.Date.Should().BeOnOrAfter(DateTime.Today).And.BeOnOrBefore(DateTime.Today.AddDays(8));
    }

    [Test]
    [Category("Venues")]
    public async Task EventsAdmin_FilteredByVenueSlug_ShowsOnlyThatVenuesEvents()
    {
        // Arrange - two venues in the same chapter, one event at each.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        var suffix = Guid.NewGuid().ToString("N");
        var (oakVenue, elmVenue) = ($"E2E Oak {suffix}", $"E2E Elm {suffix}");

        var venueAdminPage = new VenueAdminPage(Page);
        await venueAdminPage.CreateVenue(routes.VenueCreate, oakVenue, E2ESettings.VenueExternalId(0));
        await venueAdminPage.CreateVenue(routes.VenueCreate, elmVenue, E2ESettings.VenueExternalId(1));

        var oakChapterVenueId = await Venues.GetChapterVenueId(group.ChapterId, oakVenue);
        var elmChapterVenueId = await Venues.GetChapterVenueId(group.ChapterId, elmVenue);
        oakChapterVenueId.Should().NotBeNull();
        elmChapterVenueId.Should().NotBeNull();

        var (oakEvent, elmEvent) = ($"E2E Oak Event {suffix}", $"E2E Elm Event {suffix}");
        var date = $"{DateTime.Today.AddDays(14):dd/MM/yyyy} 19:00";

        var eventAdminPage = new EventAdminPage(Page);
        await eventAdminPage.CreateEvent(routes.EventCreate, oakEvent, oakChapterVenueId!.Value, date);
        await eventAdminPage.CreateEvent(routes.EventCreate, elmEvent, elmChapterVenueId!.Value, date);

        /* Act - filter by the venue's slug, which is what the query string now carries. Read rather than
           composed: a slug is built from the place the server looked up, so nothing named above predicts
           it. The two venues are two different places, which is what makes their slugs differ. */
        var oakSlug = await Venues.GetVenueSlug(group.ChapterId, oakVenue);
        oakSlug.Should().NotBeNullOrEmpty();

        var eventsAdminPage = new EventsAdminPage(Page);
        var unfiltered = await eventsAdminPage.GetEventsTableText(routes.EventsAdmin);
        var filtered = await eventsAdminPage.GetEventsTableText(routes.EventsAdmin, oakSlug!);

        // Assert - unfiltered lists both; filtering by the Oak's slug drops the Elm's event. Checking
        // the unfiltered list first means a filtered miss can't be explained by the events not existing.
        unfiltered.Should().Contain(oakEvent).And.Contain(elmEvent);
        filtered.Should().Contain(oakEvent);
        filtered.Should().NotContain(elmEvent);
    }

    [Test]
    [Category("Venues")]
    public async Task VenuesList_NameHeadingClicked_ReversesTheRowOrder()
    {
        /* Arrange - two venues whose names sort unambiguously either way. The list is already rendered
           ascending by name server-side, so what the sorting script is observably responsible for is the
           click that flips it.

           This is coverage for odk.lists.js rather than for venues: the same markup and script back the
           members and payments admin tables, and the venues list is the only one of the three whose rows a
           test can name. It earns its place because the failure it catches cannot be caught anywhere else -
           the minifier once moved the script's declarations into a block its hoisted functions could not see,
           which is a ReferenceError at runtime and perfectly valid syntax on the way there. Loading the page
           was enough to break it: the default-sort call throws before any click handler is attached, so the
           order simply never changes. */
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        var suffix = Guid.NewGuid().ToString("N");
        var (firstByName, lastByName) = ($"E2E Aaa {suffix}", $"E2E Zzz {suffix}");

        var venueAdminPage = new VenueAdminPage(Page);
        await venueAdminPage.CreateVenue(routes.VenueCreate, firstByName, E2ESettings.VenueExternalId(0));
        await venueAdminPage.CreateVenue(routes.VenueCreate, lastByName, E2ESettings.VenueExternalId(1));

        var table = new SortableTable(Page);
        await table.Open(routes.VenuesList);

        // Assert - both rows are there, ascending by name as rendered.
        var ascending = (await table.RowIndexOf(firstByName), await table.RowIndexOf(lastByName));
        ascending.Item1.Should().BeGreaterThanOrEqualTo(0, $"'{firstByName}' should be listed");
        ascending.Item2.Should().BeGreaterThanOrEqualTo(0, $"'{lastByName}' should be listed");
        ascending.Item1.Should().BeLessThan(ascending.Item2);

        // Act
        await table.SortBy("Name");

        // Assert - the same rows, the other way up.
        (await table.RowIndexOf(lastByName))
            .Should()
            .BeLessThan(
                await table.RowIndexOf(firstByName),
                "clicking the sorted column reverses it");
    }

    [Test]
    [Category("Venues")]
    public async Task CreateVenue_AsOwner_CreatesVenue()
    {
        // Arrange - an owner with a published chapter on this platform.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        // Act - the owner creates a venue, calling it something of their own.
        var suffix = Guid.NewGuid().ToString("N");
        var groupsName = $"E2E Venue {suffix}";
        await new VenueAdminPage(Page).CreateVenue(routes.VenueCreate, groupsName, E2ESettings.VenueExternalId(0));

        // Assert - the group has a venue under the name it gave.
        var exists = await Venues.VenueExists(group.ChapterId, groupsName);
        exists.Should().BeTrue();

        /* Everything the venue itself is comes from the place the server looked up, so its name is the
           place's rather than the one typed above. Asserted as a difference rather than against a literal:
           which place this is, is configuration, and the test has no business knowing what it is called. */
        var venueName = await Venues.GetVenueName(group.ChapterId, groupsName);
        venueName.Should().NotBeNullOrEmpty();
        venueName.Should().NotBe(groupsName);

        /* Likewise the slug, which is the place's name and its town. The suffix is unique to the name
           typed above, so a slug carrying it would be a slug still built from what the group called it. */
        var slug = await Venues.GetVenueSlug(group.ChapterId, groupsName);
        slug.Should().NotBeNullOrEmpty();
        slug.Should().NotContain(suffix);
    }

    [Test]
    [Category("Venues")]
    public async Task CreateVenue_NameHasStrayWhitespace_StoresItNormalised()
    {
        // Arrange - an owner with a published chapter on this platform.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        // Act - the owner types a name with stray whitespace both around it and inside it.
        var suffix = Guid.NewGuid().ToString("N");
        var venueName = $"E2E Venue {suffix}";
        await new VenueAdminPage(Page).CreateVenue(routes.VenueCreate, $"  E2E   Venue  {suffix}  ", E2ESettings.VenueExternalId(1));

        /* Assert - the group's name for the venue is stored normalised. Looking it up by that name is
           itself the assertion: any surviving stray whitespace would make it a different name and find
           nothing. Only the group's name is at stake - the venue's own name and slug are the place's,
           and nothing typed here reaches them. */
        var exists = await Venues.VenueExists(group.ChapterId, venueName);
        exists.Should().BeTrue();
    }

    /// <summary>
    /// Two groups that add the same place end up on one venue, each under its own name for it.
    /// </summary>
    /// <remarks>
    /// Only reachable end to end: reuse turns on the live lookup answering with the same name and
    /// position both times, which a stubbed places service is in no position to demonstrate. It is also
    /// deliberately invisible to a group - neither of these owners can tell the other is there - so the
    /// shared venue id is the only thing there is to assert.
    /// </remarks>
    [Test]
    [Category("Venues")]
    public async Task CreateVenue_SamePlaceAsAnotherGroup_SharesTheVenue()
    {
        // Arrange - two owners, each with a published chapter of their own.
        var (firstOwner, firstGroup) = await ProvisionOwnerChapter(GroupName());
        var (secondOwner, secondGroup) = await ProvisionOwnerChapter(GroupName());

        var suffix = Guid.NewGuid().ToString("N");
        var (firstName, secondName) = ($"E2E First {suffix}", $"E2E Second {suffix}");

        /* Act - both add the same place, each calling it something different. The first owner goes through
           a browser of their own, which is how this suite acts as a second person: signing two owners in
           and out of one browser is not what is under test here. */
        await Provisioning.CreateVenue(
            firstOwner, RoutesFor(firstGroup), firstName, E2ESettings.VenueExternalId(0), PlatformBaseUrl);

        await new LoginPage(Page).LogIn(secondOwner.Email, secondOwner.Password);
        await new VenueAdminPage(Page).CreateVenue(
            RoutesFor(secondGroup).VenueCreate, secondName, E2ESettings.VenueExternalId(0));

        // Assert - one venue, reached by two links. The second create redirecting at all is half of it:
        // "You already have this venue" refuses a place a group is on, and neither of these is.
        var firstVenueId = await Venues.GetVenueIdForChapter(firstGroup.ChapterId, firstName);
        var secondVenueId = await Venues.GetVenueIdForChapter(secondGroup.ChapterId, secondName);

        firstVenueId.Should().NotBeNull();
        secondVenueId.Should().Be(firstVenueId);
    }

    [Test]
    public async Task EventPage_Anonymous_ReturnsNotFound()
    {
        // Arrange - a published event on this platform (events are members-only by default).
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var @event = await Provisioning.CreatePublishedEvent(owner, routes, group.ChapterId, PlatformBaseUrl);

        // Act - an anonymous visitor (not logged in) opens the valid event page URL directly.
        var status = await new EventPage(Page).GetResponseStatus(routes.EventPage(@event.Shortcode));

        // Assert - the event's existence isn't leaked: it returns a 404.
        status.Should().Be(404);
    }

    [Test]
    public async Task EventPage_AsMember_DraftEventReturnsNotFound()
    {
        // Arrange - a draft (unpublished) event on this platform and a member of its chapter.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var draft = await Provisioning.CreateDraftEvent(owner, routes, group.ChapterId, PlatformBaseUrl);
        var member = await ProvisionMember(group);

        // Act - the member opens the draft event's page directly.
        await new LoginPage(Page).LogIn(member.Email, member.Password);
        var status = await new EventPage(Page).GetResponseStatus(routes.EventPage(draft.Shortcode));

        // Assert - a draft isn't visible to members: it returns a 404.
        status.Should().Be(404);
    }

    [Test]
    public async Task EventsListing_Anonymous_DoesNotShowMemberOnlyEvent()
    {
        // Arrange - a published event on this platform (events are members-only by default).
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var @event = await Provisioning.CreatePublishedEvent(owner, routes, group.ChapterId, PlatformBaseUrl);

        // Act - an anonymous visitor (not logged in) opens the events listing.
        var listed = await new EventsListPage(Page).IsEventListed(routes.EventsList, @event.Shortcode);

        // Assert - the event is not shown to anonymous visitors.
        listed.Should().BeFalse();
    }

    [Test]
    public async Task EventsListing_AsMember_DoesNotShowDraftEvent()
    {
        // Arrange - a draft (unpublished) event on this platform and a member of its chapter.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var draft = await Provisioning.CreateDraftEvent(owner, routes, group.ChapterId, PlatformBaseUrl);
        var member = await ProvisionMember(group);

        // Act - the member opens the events listing.
        await new LoginPage(Page).LogIn(member.Email, member.Password);
        var listed = await new EventsListPage(Page).IsEventListed(routes.EventsList, draft.Shortcode);

        // Assert - the draft event is not shown to the member.
        listed.Should().BeFalse();
    }

    [Test]
    public async Task EventsListing_AsMember_ShowsEvent()
    {
        // Arrange - a published event on this platform and a member of its chapter.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var @event = await Provisioning.CreatePublishedEvent(owner, routes, group.ChapterId, PlatformBaseUrl);
        var member = await ProvisionMember(group);

        // Act - the member opens the events listing.
        await new LoginPage(Page).LogIn(member.Email, member.Password);
        var listed = await new EventsListPage(Page).IsEventListed(routes.EventsList, @event.Shortcode);

        // Assert - the event is shown to the member.
        listed.Should().BeTrue();
    }

    [Test]
    public async Task Rsvp_EventFull_DoesNotRecordYes()
    {
        // Arrange - a published event limited to one attendee, with that single space already taken (the
        // owner, an approved member, fills it), and a fresh member of the chapter.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var @event = await Provisioning.CreatePublishedEvent(
            owner, routes, group.ChapterId, PlatformBaseUrl, attendeeLimit: 1);
        await EventResponses.AddAttendee(@event.EventId, owner.Email);
        var member = await ProvisionMember(group);

        // Act - the member follows the RSVP-yes link (which always attempts "yes", unlike the event page,
        // which offers the waiting list once the event is full).
        await new LoginPage(Page).LogIn(member.Email, member.Password);
        await new EventPage(Page).RsvpViaEmailLink(routes.EventRsvp(@event.Shortcode));

        // Assert - the event is full, so no "yes" is recorded for the member (they're waitlisted instead).
        var response = await EventResponses.GetResponseType(@event.EventId, member.Email);
        response.Should().BeNull();
    }

    [Test]
    public async Task Rsvp_EventHasSpace_RecordsYes()
    {
        // Arrange - a published event with spare attendee capacity and a member of its chapter.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var @event = await Provisioning.CreatePublishedEvent(
            owner, routes, group.ChapterId, PlatformBaseUrl, attendeeLimit: 5);
        var member = await ProvisionMember(group);

        // Act - the member RSVPs "yes".
        await new LoginPage(Page).LogIn(member.Email, member.Password);
        await new EventPage(Page).RsvpViaEmailLink(routes.EventRsvp(@event.Shortcode));

        // Assert - a "yes" response is recorded for the member.
        var response = await EventResponses.GetResponseType(@event.EventId, member.Email);
        response.Should().Be(ResponseYes);
    }

    [Test]
    public async Task Rsvp_ViaEventPage_RecordsYes()
    {
        // Arrange - a published event on this platform and a member of its chapter.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var @event = await Provisioning.CreatePublishedEvent(owner, routes, group.ChapterId, PlatformBaseUrl);
        var member = await ProvisionMember(group);

        // Act - the member opens the event page and clicks the "yes" RSVP control.
        await new LoginPage(Page).LogIn(member.Email, member.Password);
        await new EventPage(Page).RsvpYesOnPage(routes.EventPage(@event.Shortcode));

        // Assert - a "yes" response is recorded for the member.
        var response = await EventResponses.GetResponseType(@event.EventId, member.Email);
        response.Should().Be(ResponseYes);
    }

    [Test]
    public async Task Rsvp_ViaInviteEmailLink_RecordsYes()
    {
        // Arrange - a published event on this platform and a member of its chapter.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var @event = await Provisioning.CreatePublishedEvent(owner, routes, group.ChapterId, PlatformBaseUrl);
        var member = await ProvisionMember(group);

        // Act - the member follows the RSVP-yes link an invite email would contain.
        await new LoginPage(Page).LogIn(member.Email, member.Password);
        await new EventPage(Page).RsvpViaEmailLink(routes.EventRsvp(@event.Shortcode));

        // Assert - a "yes" response is recorded for the member.
        var response = await EventResponses.GetResponseType(@event.EventId, member.Email);
        response.Should().Be(ResponseYes);
    }

    [Test]
    public async Task UpdateEvent_ChangeName_PersistsChange()
    {
        // Arrange - an owner with a published chapter and an existing event to edit.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var @event = await Provisioning.CreatePublishedEvent(owner, routes, group.ChapterId, PlatformBaseUrl);
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        // Act - open the event's edit page, change its name, and submit the update. The edit form posts
        // back to itself and must carry an antiforgery token; without it the POST is a 400.
        var newName = $"E2E Event Updated {Guid.NewGuid():N}";
        await new EventAdminPage(Page).UpdateEventName(routes.EventEdit(@event.EventId), newName);

        // Assert - the new name is persisted (a CSRF/antiforgery failure would leave it unchanged).
        (await Events.GetName(@event.EventId)).Should().Be(newName);
    }

    [Test]
    public async Task UpdateEventSettings_SetDefaultDayAndTime_PersistsSettings()
    {
        // Arrange - an owner with a published chapter on this platform.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        // Act - set the default event day of week and start time.
        await new EventSettingsPage(Page).SetDefaults(routes.EventSettings, DayOfWeek.Wednesday, "19:00");

        // Assert - persisted on the chapter's event settings (DefaultDayOfWeek is the .NET DayOfWeek int).
        (await EventSettingsData.GetDefaultDayOfWeek(group.ChapterId)).Should().Be((int)DayOfWeek.Wednesday);
        (await EventSettingsData.GetDefaultStartTime(group.ChapterId)).Should().Be(new TimeSpan(19, 0, 0));
    }

    private protected abstract Task<TestAccount> ProvisionMember(TestGroup group);

    private protected abstract Task<(TestAccount Owner, TestGroup Group)> ProvisionOwnerChapter(string name);

    private protected abstract PlatformRoutes RoutesFor(TestGroup group);

    // A URL-safe, space-free group name: the DrunkenKnitwits route segment is the chapter's ShortName,
    // derived from the name.
    private static string GroupName() => $"e2eevt{Guid.NewGuid():N}";
}

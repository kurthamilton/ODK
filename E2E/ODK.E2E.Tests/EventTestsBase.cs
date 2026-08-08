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
        await new VenueAdminPage(Page).CreateVenue(routes.VenueCreate, venueName);
        var venueId = await Venues.GetVenueId(group.ChapterId, venueName);
        venueId.Should().NotBeNull();

        // Act - create an event setting only the required fields (Name, Venue, Date).
        var eventName = $"E2E Event {Guid.NewGuid():N}";
        var date = $"{DateTime.Today.AddDays(14):dd/MM/yyyy} 19:00";
        await new EventAdminPage(Page).CreateEvent(routes.EventCreate, eventName, venueId!.Value, date);

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
        await new VenueAdminPage(Page).CreateVenue(routes.VenueCreate, $"E2E Venue {Guid.NewGuid():N}");

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
        await venueAdminPage.CreateVenue(routes.VenueCreate, oakVenue);
        await venueAdminPage.CreateVenue(routes.VenueCreate, elmVenue);

        var oakVenueId = await Venues.GetVenueId(group.ChapterId, oakVenue);
        var elmVenueId = await Venues.GetVenueId(group.ChapterId, elmVenue);
        oakVenueId.Should().NotBeNull();
        elmVenueId.Should().NotBeNull();

        var (oakEvent, elmEvent) = ($"E2E Oak Event {suffix}", $"E2E Elm Event {suffix}");
        var date = $"{DateTime.Today.AddDays(14):dd/MM/yyyy} 19:00";

        var eventAdminPage = new EventAdminPage(Page);
        await eventAdminPage.CreateEvent(routes.EventCreate, oakEvent, oakVenueId!.Value, date);
        await eventAdminPage.CreateEvent(routes.EventCreate, elmEvent, elmVenueId!.Value, date);

        // Act - filter by the venue's slug, which is what the query string now carries.
        var eventsAdminPage = new EventsAdminPage(Page);
        var unfiltered = await eventsAdminPage.GetEventsTableText(routes.EventsAdmin);
        var filtered = await eventsAdminPage.GetEventsTableText(routes.EventsAdmin, $"e2e-oak-{suffix}");

        // Assert - unfiltered lists both; filtering by the Oak's slug drops the Elm's event. Checking
        // the unfiltered list first means a filtered miss can't be explained by the events not existing.
        unfiltered.Should().Contain(oakEvent).And.Contain(elmEvent);
        filtered.Should().Contain(oakEvent);
        filtered.Should().NotContain(elmEvent);
    }

    [Test]
    [Category("Venues")]
    public async Task CreateVenue_AsOwner_CreatesVenue()
    {
        // Arrange - an owner with a published chapter on this platform.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        // Act - the owner creates a venue.
        var suffix = Guid.NewGuid().ToString("N");
        var venueName = $"E2E Venue {suffix}";
        await new VenueAdminPage(Page).CreateVenue(routes.VenueCreate, venueName);

        // Assert - the venue now exists for the chapter, slugged from its name.
        var exists = await Venues.VenueExists(group.ChapterId, venueName);
        exists.Should().BeTrue();

        // The expected slug is spelled out rather than derived, so the test pins the real rules
        // (lowercased, spaces to hyphens) instead of restating the app's implementation of them.
        var slug = await Venues.GetVenueSlug(group.ChapterId, venueName);
        slug.Should().Be($"e2e-venue-{suffix}");
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
        await new VenueAdminPage(Page).CreateVenue(routes.VenueCreate, $"  E2E   Venue  {suffix}  ");

        // Assert - the venue is stored under the normalised name. Looking it up by that name is itself
        // the assertion: any surviving stray whitespace would make it a different name and find nothing.
        var exists = await Venues.VenueExists(group.ChapterId, venueName);
        exists.Should().BeTrue();

        var slug = await Venues.GetVenueSlug(group.ChapterId, venueName);
        slug.Should().Be($"e2e-venue-{suffix}");
    }

    [Test]
    [Category("Venues")]
    public async Task CreateVenue_NameSlugsToAnExistingSlug_VersionsTheSlugAndStillCreates()
    {
        // Arrange - an owner with a published chapter on this platform.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        // A venue name is unique within a chapter, so two venues can never share one - the collision
        // has to come from two *different* names that slug to the same value. Trailing punctuation is
        // dropped by the slug rules, so these two names differ (satisfying the unique index) while both
        // slugging to "e2e-venue-{suffix}".
        var suffix = Guid.NewGuid().ToString("N");
        var firstName = $"E2E Venue {suffix}";
        var secondName = $"E2E Venue {suffix}!";
        var venueAdminPage = new VenueAdminPage(Page);

        // Act - create both. CreateVenue throws if the form fails to redirect, so the second call
        // reaching the venues list is itself the assertion that a collision doesn't block creation.
        await venueAdminPage.CreateVenue(routes.VenueCreate, firstName);
        await venueAdminPage.CreateVenue(routes.VenueCreate, secondName);

        // Assert - both exist, the first keeps the unversioned slug, and the second is versioned.
        var firstSlug = await Venues.GetVenueSlug(group.ChapterId, firstName);
        var secondSlug = await Venues.GetVenueSlug(group.ChapterId, secondName);

        firstSlug.Should().Be($"e2e-venue-{suffix}");
        secondSlug.Should().Be($"e2e-venue-{suffix}-2");
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

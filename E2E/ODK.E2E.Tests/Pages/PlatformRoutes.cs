using ODK.E2E.Data.Models;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// Builds the platform-correct <em>relative</em> URLs a test drives (resolved against the fixture's
/// context BaseURL). The same admin function lives under different route trees per platform - Default
/// under <c>/my/groups/{chapterId}/...</c>, DrunkenKnitwits under <c>/{chapterName}/admin/...</c>, and
/// the leaf segments even differ (<c>/new</c> vs <c>/create</c>) - so page objects take the finished
/// path from here and stay platform-agnostic. Mirrors the app's <c>GroupAdminRoutes</c>/<c>GroupRoutes</c>.
/// </summary>
internal abstract class PlatformRoutes
{
    /// <summary>
    /// Admin: the group's email-templates list, which links to an edit page per template the site allows a
    /// group to override.
    /// </summary>
    public abstract string EmailsAdmin { get; }

    /// <summary>Admin: create-event page (also the POST target - it posts back to itself).</summary>
    public abstract string EventCreate { get; }

    /// <summary>Admin: the events list page, which carries the venue/date filters.</summary>
    public abstract string EventsAdmin { get; }

    /// <summary>Admin: event settings page (default day of week, start time, etc.).</summary>
    public abstract string EventSettings { get; }

    /// <summary>Admin: create-venue page (also the POST target - it posts back to itself).</summary>
    public abstract string VenueCreate { get; }

    /// <summary>Member-facing: the chapter/group contact page.</summary>
    public abstract string Contact { get; }

    /// <summary>Member-facing: the upcoming-events listing page.</summary>
    public abstract string EventsList { get; }

    /// <summary>Member-facing: the chapter/group landing (home) page.</summary>
    public abstract string Home { get; }

    /// <summary>Admin: the members list, which is also where a completed import redirects.</summary>
    public abstract string MembersAdmin { get; }

    /// <summary>Admin: the member-import wizard (upload a CSV, review, confirm).</summary>
    public abstract string MembersImport { get; }

    /// <summary>Member-facing: the members-list page.</summary>
    public abstract string MembersList { get; }

    /// <summary>Admin: create member-profile-property page (also the POST target - posts back to itself).</summary>
    public abstract string PropertyCreate { get; }

    /// <summary>Admin: member-profile-properties list page (carries the move-up/down reorder controls).</summary>
    public abstract string PropertiesList { get; }

    /// <summary>Member-facing: the current member's own profile-update form for this chapter.</summary>
    public abstract string ProfileUpdate { get; }

    /// <summary>Admin: create chapter-subscription page (also the POST target - it posts back to itself).</summary>
    public abstract string SubscriptionCreate { get; }

    /// <summary>Admin: chapter-subscriptions list page (the create-success redirect target).</summary>
    public abstract string SubscriptionsList { get; }

    public static PlatformRoutes Default(TestGroup group) => new DefaultPlatformRoutes(group);

    public static PlatformRoutes DrunkenKnitwits(TestGroup group) => new DrunkenKnitwitsPlatformRoutes(group);

    /// <summary>Admin: edit-event page (also the POST target - it posts back to itself).</summary>
    public abstract string EventEdit(Guid eventId);

    /// <summary>Member-facing: the event detail page (carries the on-page RSVP control).</summary>
    public abstract string EventPage(string shortcode);

    /// <summary>Member-facing: the "RSVP yes" link an event invite email would contain.</summary>
    public abstract string EventRsvp(string shortcode);

    /// <summary>Member-facing: a member's profile page (their answers, shown to fellow members).</summary>
    public abstract string MemberPage(Guid memberId);

    /// <summary>Member-facing: the checkout page for purchasing the given chapter subscription.</summary>
    public abstract string SubscriptionCheckout(Guid chapterSubscriptionId);

    private sealed class DefaultPlatformRoutes : PlatformRoutes
    {
        private readonly Guid _chapterId;
        private readonly string _slug;

        public DefaultPlatformRoutes(TestGroup group)
        {
            _chapterId = group.ChapterId;
            _slug = group.Slug;
        }

        public override string EmailsAdmin => $"/my/groups/{_chapterId}/emails";

        public override string EventCreate => $"/my/groups/{_chapterId}/events/new";

        public override string EventsAdmin => $"/my/groups/{_chapterId}/events";

        public override string EventSettings => $"/my/groups/{_chapterId}/events/settings";

        public override string VenueCreate => $"/my/groups/{_chapterId}/events/venues/new";

        public override string Contact => $"/groups/{_slug}/contact";

        public override string EventsList => $"/groups/{_slug}/events";

        public override string Home => $"/groups/{_slug}";

        public override string MembersAdmin => $"/my/groups/{_chapterId}/members";

        public override string MembersImport => $"/my/groups/{_chapterId}/members/import";

        public override string MembersList => $"/groups/{_slug}/members";

        public override string PropertyCreate => $"/my/groups/{_chapterId}/members/properties/new";

        public override string PropertiesList => $"/my/groups/{_chapterId}/members/properties";

        public override string ProfileUpdate => $"/groups/{_slug}/profile";

        public override string SubscriptionCreate => $"/my/groups/{_chapterId}/members/subscriptions/new";

        public override string SubscriptionsList => $"/my/groups/{_chapterId}/members/subscriptions";

        public override string EventEdit(Guid eventId) => $"/my/groups/{_chapterId}/events/{eventId}";

        public override string EventPage(string shortcode) => $"/groups/{_slug}/events/{shortcode}";

        public override string EventRsvp(string shortcode) => $"/groups/{_slug}/events/{shortcode}/rsvp";

        public override string MemberPage(Guid memberId) => $"/groups/{_slug}/members/{memberId}";

        public override string SubscriptionCheckout(Guid chapterSubscriptionId) =>
            $"/groups/{_slug}/subscription/{chapterSubscriptionId}/checkout";
    }

    private sealed class DrunkenKnitwitsPlatformRoutes : PlatformRoutes
    {
        private readonly string _shortName;

        public DrunkenKnitwitsPlatformRoutes(TestGroup group)
        {
            // The DrunkenKnitwits URL segment is the chapter's ShortName - the (un-suffixed) name the
            // chapter was created with, lowercased. TestGroup.Name is that un-suffixed name.
            _shortName = group.Name.ToLowerInvariant();
        }

        public override string EmailsAdmin => $"/{_shortName}/admin/chapter/emails";

        public override string EventCreate => $"/{_shortName}/admin/events/create";

        public override string EventsAdmin => $"/{_shortName}/admin/events";

        public override string EventSettings => $"/{_shortName}/admin/events/settings";

        public override string VenueCreate => $"/{_shortName}/admin/events/venues/create";

        public override string Contact => $"/{_shortName}/contact";

        public override string EventsList => $"/{_shortName}/events";

        public override string Home => $"/{_shortName}";

        public override string MembersAdmin => $"/{_shortName}/admin/members";

        public override string MembersImport => $"/{_shortName}/admin/members/import";

        public override string MembersList => $"/{_shortName}/members";

        public override string PropertyCreate => $"/{_shortName}/admin/members/properties/create";

        public override string PropertiesList => $"/{_shortName}/admin/members/properties";

        public override string ProfileUpdate => $"/{_shortName}/account/profile";

        public override string SubscriptionCreate => $"/{_shortName}/admin/members/subscriptions/create";

        public override string SubscriptionsList => $"/{_shortName}/admin/members/subscriptions";

        public override string EventEdit(Guid eventId) => $"/{_shortName}/admin/events/{eventId}";

        public override string EventPage(string shortcode) => $"/{_shortName}/events/{shortcode}";

        public override string EventRsvp(string shortcode) => $"/{_shortName}/events/{shortcode}/rsvp";

        public override string MemberPage(Guid memberId) => $"/{_shortName}/members/{memberId}";

        public override string SubscriptionCheckout(Guid chapterSubscriptionId) =>
            $"/{_shortName}/account/subscription/{chapterSubscriptionId}/checkout";
    }
}

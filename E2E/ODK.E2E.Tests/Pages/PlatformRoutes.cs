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
    public static PlatformRoutes Default(TestGroup group) => new DefaultPlatformRoutes(group);

    public static PlatformRoutes DrunkenKnitwits(TestGroup group) => new DrunkenKnitwitsPlatformRoutes(group);

    /// <summary>Admin: create-event page (also the POST target - it posts back to itself).</summary>
    public abstract string EventCreate { get; }

    /// <summary>Admin: event settings page (default day of week, start time, etc.).</summary>
    public abstract string EventSettings { get; }

    /// <summary>Admin: create-venue page (also the POST target - it posts back to itself).</summary>
    public abstract string VenueCreate { get; }

    /// <summary>Member-facing: the upcoming-events listing page.</summary>
    public abstract string EventsList { get; }

    /// <summary>Member-facing: the event detail page (carries the on-page RSVP control).</summary>
    public abstract string EventPage(string shortcode);

    /// <summary>Member-facing: the "RSVP yes" link an event invite email would contain.</summary>
    public abstract string EventRsvp(string shortcode);

    private sealed class DefaultPlatformRoutes : PlatformRoutes
    {
        private readonly Guid _chapterId;
        private readonly string _slug;

        public DefaultPlatformRoutes(TestGroup group)
        {
            _chapterId = group.ChapterId;
            _slug = group.Slug;
        }

        public override string EventCreate => $"/my/groups/{_chapterId}/events/new";

        public override string EventSettings => $"/my/groups/{_chapterId}/events/settings";

        public override string VenueCreate => $"/my/groups/{_chapterId}/events/venues/new";

        public override string EventsList => $"/groups/{_slug}/events";

        public override string EventPage(string shortcode) => $"/groups/{_slug}/events/{shortcode}";

        public override string EventRsvp(string shortcode) => $"/groups/{_slug}/events/{shortcode}/rsvp";
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

        public override string EventCreate => $"/{_shortName}/admin/events/create";

        public override string EventSettings => $"/{_shortName}/admin/events/settings";

        public override string VenueCreate => $"/{_shortName}/admin/events/venues/create";

        public override string EventsList => $"/{_shortName}/events";

        public override string EventPage(string shortcode) => $"/{_shortName}/events/{shortcode}";

        public override string EventRsvp(string shortcode) => $"/{_shortName}/events/{shortcode}/rsvp";
    }
}

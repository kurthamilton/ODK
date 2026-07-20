namespace ODK.Data.Core.Events;

/// <summary>
/// Server-side filters for the admin events listing. Applied to all of a chapter's events, not just
/// the current page. Extend with new filters as needed.
/// </summary>
public class EventAdminFilter
{
    public DateTime? FromDate { get; init; }

    public DateTime? ToDate { get; init; }

    public Guid? VenueId { get; init; }
}

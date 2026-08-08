namespace ODK.Data.Core.Events;

public class EventAdminFilter
{
    public DateTime? FromDateLocal { get; init; }

    public DateTime? ToDateLocal { get; init; }

    /// <summary>
    /// The venue's slug, so the filtered URL stays readable
    /// </summary>
    public string? VenueSlug { get; init; }
}

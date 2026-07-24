namespace ODK.Data.Core.Events;

public class EventAdminFilter
{
    public DateTime? FromDateLocal { get; init; }

    public DateTime? ToDateLocal { get; init; }

    public Guid? VenueId { get; init; }
}

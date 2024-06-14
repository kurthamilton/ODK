namespace ODK.Services.Venues;

public class VenueStats
{
    public int EventCount { get; set; }

    public DateTime? LastEventDate { get; set; }

    public Guid VenueId { get; set; }
}

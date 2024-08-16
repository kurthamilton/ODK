namespace ODK.Services.Events;

public class CreateEvent
{
    public required int? AttendeeLimit { get; set; }

    public required DateTime Date { get; set; }

    public required string? Description { get; set; }

    public required List<Guid> Hosts { get; set; } = new();

    public required string? ImageUrl { get; set; }

    public required bool IsPublic { get; set; }

    public required string Name { get; set; } = "";

    public required decimal? TicketCost { get; set; }

    public required decimal? TicketDepositCost { get; set; }

    public required string? Time { get; set; }

    public required Guid VenueId { get; set; }
}

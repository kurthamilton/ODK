namespace ODK.Services.Events;

public class CreateEvent
{
    public required int? AttendeeLimit { get; init; }

    public required DateTime Date { get; init; }

    public required string? Description { get; init; }

    public required List<Guid> Hosts { get; init; } = new();

    public required string? ImageUrl { get; init; }

    public required bool IsPublic { get; init; }

    public required string Name { get; init; } = "";

    public required DateTime? RsvpDeadline { get; init; }

    public required decimal? TicketCost { get; init; }

    public required decimal? TicketDepositCost { get; init; }

    public required string? Time { get; init; }

    public required Guid VenueId { get; init; }
}

namespace ODK.Services.Events;

public class EventInvitesDto
{
    public required Guid EventId { get; set; }

    public required int Sent { get; set; }

    public required DateTime? ScheduledUtc { get; set; }

    public required DateTime? SentUtc { get; set; }
}

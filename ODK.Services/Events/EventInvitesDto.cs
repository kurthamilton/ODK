namespace ODK.Services.Events;

public class EventInvitesDto
{
    public required Guid EventId { get; set; }

    public required int Sent { get; set; }

    public required DateTime? ScheduledDate { get; set; }

    public required DateTime? SentDate { get; set; }
}

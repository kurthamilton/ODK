namespace ODK.Services.Events;

public class EventInvitesDto
{
    public Guid EventId { get; set; }

    public int Sent { get; set; }

    public DateTime? SentDate { get; set; }
}

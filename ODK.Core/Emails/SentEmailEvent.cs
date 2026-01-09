namespace ODK.Core.Emails;

public class SentEmailEvent : IDatabaseEntity
{
    public DateTime CreatedUtc { get; set; }

    public required string EventName { get; set; }

    public Guid Id { get; set; }

    public Guid SentEmailId { get; set; }
}

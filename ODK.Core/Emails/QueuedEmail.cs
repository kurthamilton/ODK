namespace ODK.Core.Emails;

public class QueuedEmail : IDatabaseEntity
{
    public string Body { get; set; } = "";

    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public DateTime? SendAfterUtc { get; set; }

    public string Subject { get; set; } = "";
}

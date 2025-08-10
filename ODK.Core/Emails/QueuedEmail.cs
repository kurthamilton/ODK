namespace ODK.Core.Emails;

public class QueuedEmail : IDatabaseEntity
{
    public string Body { get; set; } = "";

    public Guid? ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public string FromEmailAddress { get; set; } = "";

    public string FromName { get; set; } = "";

    public Guid Id { get; set; }

    public DateTime? SendAfterUtc { get; set; }

    public string Subject { get; set; } = "";
}

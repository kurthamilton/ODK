namespace ODK.Core.Emails;

public class QueuedEmailRecipient : IDatabaseEntity
{
    public string EmailAddress { get; set; } = "";

    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public Guid QueuedEmailId { get; set; }
}

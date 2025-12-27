namespace ODK.Core.Emails;

public class QueuedEmailRecipient : IDatabaseEntity
{
    public string EmailAddress { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid QueuedEmailId { get; set; }
}

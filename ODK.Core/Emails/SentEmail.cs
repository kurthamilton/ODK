namespace ODK.Core.Emails;

public class SentEmail : IDatabaseEntity
{
    public Guid? ChapterEmailProviderId { get; set; }

    public Guid? EmailProviderId { get; set; }

    public string? ExternalId { get; set; }

    public Guid Id { get; set; }

    public DateTime SentUtc { get; set; }    

    public string Subject { get; set; } = string.Empty;

    public string To { get; set; } = string.Empty;
}

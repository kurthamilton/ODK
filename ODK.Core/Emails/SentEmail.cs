namespace ODK.Core.Emails;

public class SentEmail
{
    public Guid ChapterEmailProviderId { get; set; }

    public DateTime SentDate { get; set; }

    public string Subject { get; set; } = "";

    public string To { get; set; } = "";
}

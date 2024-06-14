namespace ODK.Core.Emails;

public class SentEmail
{
    public SentEmail(Guid chapterEmailProviderId, DateTime sentDate, string to, string subject)
    {
        ChapterEmailProviderId = chapterEmailProviderId;
        SentDate = sentDate;
        Subject = subject;
        To = to;
    }

    public Guid ChapterEmailProviderId { get; }

    public DateTime SentDate { get; }

    public string Subject { get; set; }

    public string To { get; }
}

namespace ODK.Core.Emails;

public class ChapterEmail : IDatabaseEntity
{
    public Guid ChapterId { get; set; }

    public string HtmlContent { get; set; } = "";

    public Guid Id { get; set; }

    public string Subject { get; set; } = "";

    public EmailType Type { get; set; }

    public bool IsDefault() => Id == Guid.Empty;

    public Email ToEmail()
    {
        return new Email
        {
            HtmlContent = HtmlContent,
            Subject = Subject,
            Type = Type
        };
    }
}

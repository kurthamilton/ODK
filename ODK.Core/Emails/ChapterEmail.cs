namespace ODK.Core.Emails;

public class ChapterEmail : IDatabaseEntity
{
    public Guid ChapterId { get; set; }

    public string HtmlContent { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string Subject { get; set; } = string.Empty;

    public EmailType Type { get; set; }

    public bool IsDefault() => Id == default;

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

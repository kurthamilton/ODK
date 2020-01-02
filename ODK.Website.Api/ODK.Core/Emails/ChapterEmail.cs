using System;

namespace ODK.Core.Emails
{
    public class ChapterEmail
    {
        public ChapterEmail(Guid id, Guid chapterId, EmailType type, string subject, string htmlContent)
        {
            ChapterId = chapterId;
            HtmlContent = htmlContent;
            Id = id;
            Subject = subject;
            Type = type;
        }

        public Guid ChapterId { get; }

        public string HtmlContent { get; set; }

        public Guid Id { get; }

        public string Subject { get; set; }

        public EmailType Type { get; }
    }
}

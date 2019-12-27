using System;
using ODK.Core.Mail;

namespace ODK.Web.Api.Admin.Chapters.Responses
{
    public class ChapterEmailApiResponse
    {
        public string HtmlContent { get; set; }

        public Guid? Id { get; set; }

        public string Subject { get; set; }

        public EmailType Type { get; set; }
    }
}

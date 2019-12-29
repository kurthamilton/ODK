using ODK.Core.Mail;

namespace ODK.Web.Api.Admin.Emails.Responses
{
    public class EmailApiResponse
    {
        public string HtmlContent { get; set; }

        public string Subject { get; set; }

        public EmailType Type { get; set; }
    }
}

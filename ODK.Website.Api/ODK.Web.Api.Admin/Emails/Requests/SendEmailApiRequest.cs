using System;

namespace ODK.Web.Api.Admin.Emails.Requests
{
    public class SendEmailApiRequest
    {
        public string Body { get; set; }

        public Guid MemberId { get; set; }

        public string Subject { get; set; }
    }
}

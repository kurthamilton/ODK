using System;

namespace ODK.Web.Api.Admin.Emails.Responses
{
    public class ChapterEmailProviderApiResponse
    {
        public int? BatchSize { get; set; }

        public int DailyLimit { get; set; }

        public string FromEmailAddress { get; set; }

        public string FromName { get; set; }

        public Guid Id { get; set; }

        public int Order { get; set; }

        public string SmtpLogin { get; set; }

        public string SmtpPassword { get; set; }

        public int SmtpPort { get; set; }

        public string SmtpServer { get; set; }
    }
}

﻿namespace ODK.Services.Emails
{
    public class UpdateChapterEmailProviderSettings
    {
        public string FromEmailAddress { get; set; }

        public string FromName { get; set; }

        public string SmtpLogin { get; set; }

        public string SmtpPassword { get; set; }

        public int SmtpPort { get; set; }

        public string SmtpServer { get; set; }
    }
}
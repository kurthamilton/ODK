using System;

namespace ODK.Core.Chapters
{
    public class ChapterEmailProviderSettings
    {
        public ChapterEmailProviderSettings(Guid chapterId)
        {
            ChapterId = chapterId;
        }

        public ChapterEmailProviderSettings(Guid chapterId, 
            string smtpServer, int smtpPort, string smtpLogin, string smtpPassword,
            string fromEmailAddress, string fromName)
            : this(chapterId)
        {
            FromEmailAddress = fromEmailAddress;
            FromName = fromName;
            SmtpLogin = smtpLogin;
            SmtpPassword = smtpPassword;
            SmtpPort = smtpPort;
            SmtpServer = smtpServer;
        }

        public Guid ChapterId { get; }

        public string FromEmailAddress { get; set; }

        public string FromName { get; set; }

        public string SmtpLogin { get; set; }

        public string SmtpPassword { get; set; }

        public int SmtpPort { get; set; }

        public string SmtpServer { get; set; }
    }
}

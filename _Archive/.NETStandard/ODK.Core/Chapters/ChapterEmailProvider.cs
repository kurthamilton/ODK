using System;

namespace ODK.Core.Chapters
{
    public class ChapterEmailProvider
    {
        public ChapterEmailProvider(Guid id, Guid chapterId)
        {
            ChapterId = chapterId;
            Id = id;
        }

        public ChapterEmailProvider(Guid id, Guid chapterId,
            string smtpServer, int smtpPort, string smtpLogin, string smtpPassword,
            string fromEmailAddress, string fromName, int? batchSize, int dailyLimit, int order)
            : this(id, chapterId)
        {
            BatchSize = batchSize;
            DailyLimit = dailyLimit;
            FromEmailAddress = fromEmailAddress;
            FromName = fromName;
            Order = order;
            SmtpLogin = smtpLogin;
            SmtpPassword = smtpPassword;
            SmtpPort = smtpPort;
            SmtpServer = smtpServer;
        }

        public int? BatchSize { get; set; }

        public Guid ChapterId { get; }

        public int DailyLimit { get; set; }

        public string FromEmailAddress { get; set; } = "";

        public string FromName { get; set; } = "";

        public Guid Id { get; }

        public int Order { get; set; }

        public string SmtpLogin { get; set; } = "";

        public string SmtpPassword { get; set; } = "";

        public int SmtpPort { get; set; }

        public string SmtpServer { get; set; } = "";
    }
}

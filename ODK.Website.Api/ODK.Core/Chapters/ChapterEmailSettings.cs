using System;

namespace ODK.Core.Chapters
{
    public class ChapterEmailSettings
    {
        public ChapterEmailSettings(Guid chapterId, string adminEmailAddress, string contactEmailAddress,
            string fromEmailAddress, string fromEmailName, string emailProvider, string emailApiKey)
        {
            AdminEmailAddress = adminEmailAddress;
            ChapterId = chapterId;
            ContactEmailAddress = contactEmailAddress;
            EmailApiKey = emailApiKey;
            EmailProvider = emailProvider;
            FromEmailAddress = fromEmailAddress;
            FromEmailName = fromEmailName;
        }

        public string AdminEmailAddress { get; set; }

        public Guid ChapterId { get; }

        public string ContactEmailAddress { get; set; }

        public string EmailApiKey { get; set; }

        public string EmailProvider { get; set; }

        public string FromEmailAddress { get; }

        public string FromEmailName { get; }
    }
}

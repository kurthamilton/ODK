using System;

namespace ODK.Core.Chapters
{
    public class ChapterEmailSettings
    {
        public ChapterEmailSettings(Guid chapterId, string adminEmailAddress, string contactEmailAddress, 
            string fromEmailAddress, string emailProvider, string emailApiKey)
        {
            AdminEmailAddress = adminEmailAddress;
            ChapterId = chapterId;
            ContactEmailAddress = contactEmailAddress;
            EmailApiKey = emailApiKey;
            EmailProvider = emailProvider;
            FromEmailAddress = fromEmailAddress;
        }

        public string AdminEmailAddress { get; }

        public Guid ChapterId { get; }

        public string ContactEmailAddress { get; }

        public string EmailApiKey { get; }

        public string EmailProvider { get; }

        public string FromEmailAddress { get; }
    }
}

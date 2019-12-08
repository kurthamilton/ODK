using System;

namespace ODK.Core.Chapters
{
    public class ChapterEmailSettings
    {
        public ChapterEmailSettings(Guid chapterId, string adminEmailAddress, string contactEmailAddress, 
            string fromEmailAddress)
        {
            AdminEmailAddress = adminEmailAddress;
            ChapterId = chapterId;
            ContactEmailAddress = contactEmailAddress;
            FromEmailAddress = fromEmailAddress;
        }

        public string AdminEmailAddress { get; }

        public Guid ChapterId { get; }

        public string ContactEmailAddress { get; }

        public string FromEmailAddress { get; }
    }
}

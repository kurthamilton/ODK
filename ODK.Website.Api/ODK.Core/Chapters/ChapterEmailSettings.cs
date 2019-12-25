using System;

namespace ODK.Core.Chapters
{
    public class ChapterEmailSettings
    {
        public ChapterEmailSettings(Guid chapterId, string adminEmailAddress, string contactEmailAddress)
        {
            AdminEmailAddress = adminEmailAddress;
            ChapterId = chapterId;
            ContactEmailAddress = contactEmailAddress;
        }

        public string AdminEmailAddress { get; set; }

        public Guid ChapterId { get; }

        public string ContactEmailAddress { get; set; }
    }
}

using System;

namespace ODK.Core.Chapters
{
    public class ChapterEmailSettings
    {
        public ChapterEmailSettings(Guid chapterId, string contactEmailAddress, string fromEmailAddress)
        {
            ChapterId = chapterId;
            ContactEmailAddress = contactEmailAddress;
            FromEmailAddress = fromEmailAddress;
        }

        public Guid ChapterId { get; }

        public string ContactEmailAddress { get; }

        public string FromEmailAddress { get; }
    }
}

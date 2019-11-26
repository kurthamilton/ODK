using System;

namespace ODK.Core.Chapters
{
    public class ChapterEmailSettings
    {
        public ChapterEmailSettings(Guid chapterId, string fromEmailAddress)
        {
            ChapterId = chapterId;
            FromEmailAddress = fromEmailAddress;
        }

        public Guid ChapterId { get; }

        public string FromEmailAddress { get; }
    }
}

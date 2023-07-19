using System;

namespace ODK.Core.Chapters
{
    public class ChapterLinks : IVersioned
    {
        public ChapterLinks(Guid chapterId, string facebookName, string instagramName, string twitterName, long version)
        {
            ChapterId = chapterId;
            FacebookName = facebookName;
            InstagramName = instagramName;
            TwitterName = twitterName;
            Version = version;
        }

        public Guid ChapterId { get; }

        public string FacebookName { get; }

        public string InstagramName { get; }

        public string TwitterName { get; }

        public long Version { get; }
    }
}

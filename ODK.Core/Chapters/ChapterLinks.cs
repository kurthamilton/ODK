using System;
using System.Collections.Generic;
using System.Text;

namespace ODK.Core.Chapters
{
    public class ChapterLinks
    {
        public ChapterLinks(Guid chapterId, string facebookName, string instagramName, string twitterName)
        {
            ChapterId = chapterId;
            FacebookName = facebookName;
            InstagramName = instagramName;
            TwitterName = twitterName;
        }

        public Guid ChapterId { get; }

        public string FacebookName { get; }

        public string InstagramName { get; }

        public string TwitterName { get; }
    }
}

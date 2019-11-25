using System;

namespace ODK.Core.Chapters
{
    public class ChapterAdminMember
    {
        public ChapterAdminMember(Guid chapterId, Guid memberId)
        {
            ChapterId = chapterId;
            MemberId = memberId;
        }

        public Guid ChapterId { get; }

        public Guid MemberId { get; }
    }
}

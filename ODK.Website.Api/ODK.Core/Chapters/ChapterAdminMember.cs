using System;

namespace ODK.Core.Chapters
{
    public class ChapterAdminMember
    {
        public ChapterAdminMember(Guid chapterId, Guid memberId, bool superAdmin)
        {
            ChapterId = chapterId;
            MemberId = memberId;
            SuperAdmin = superAdmin;
        }

        public Guid ChapterId { get; }

        public Guid MemberId { get; }

        public bool SuperAdmin { get; }
    }
}

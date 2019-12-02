using System;
using System.Data;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterAdminMemberMap : SqlMap<ChapterAdminMember>
    {
        public ChapterAdminMemberMap()
            : base("ChapterAdminMembers")
        {
            Property(x => x.ChapterId);
            Property(x => x.MemberId);
            Property(x => x.SuperAdmin).FromTable("Members");

            Join<Member, Guid>(x => x.MemberId, x => x.Id);
        }

        public override ChapterAdminMember Read(IDataReader reader)
        {
            return new ChapterAdminMember
            (
                chapterId: reader.GetGuid(0),
                memberId: reader.GetGuid(1),
                superAdmin: reader.GetBoolean(2)
            );
        }
    }
}

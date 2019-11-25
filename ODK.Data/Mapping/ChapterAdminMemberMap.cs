using System.Data;
using ODK.Core.Chapters;
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
        }

        public override ChapterAdminMember Read(IDataReader reader)
        {
            return new ChapterAdminMember
            (
                chapterId: reader.GetGuid(0),
                memberId: reader.GetGuid(1)
            );
        }
    }
}

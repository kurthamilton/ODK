using System.Data;
using ODK.Core.Members;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class MemberGroupMemberMap : SqlMap<MemberGroupMember>
    {
        public MemberGroupMemberMap()
            : base("MemberGroupMembers")
        {
            Property(x => x.MemberGroupId);
            Property(x => x.MemberId);
        }

        public override MemberGroupMember Read(IDataReader reader)
        {
            return new MemberGroupMember
            (
                memberGroupId: reader.GetGuid(0),
                memberId: reader.GetGuid(1)
            );
        }
    }
}

using System;

namespace ODK.Core.Members
{
    public class MemberGroupMember
    {
        public MemberGroupMember(Guid memberGroupId, Guid memberId)
        {
            MemberGroupId = memberGroupId;
            MemberId = memberId;
        }

        public Guid MemberGroupId { get; }

        public Guid MemberId { get; }
    }
}

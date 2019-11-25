using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Members;

namespace ODK.Services.Members
{
    public interface IMemberService
    {
        Task<Member> GetMember(Guid currentMemberId, Guid memberId);

        Task<MemberImage> GetMemberImage(Guid currentMemberId, Guid memberId);

        Task<MemberProfile> GetMemberProfile(Guid currentMemberId, Guid memberId);

        Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId);

        Task UpdateMemberImage(MemberImage image);

        Task UpdateMemberProfile(UpdateMemberProfile profile);
    }
}

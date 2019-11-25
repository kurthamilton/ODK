using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Members;

namespace ODK.Services.Members
{
    public interface IMemberService
    {
        Task<MemberImage> GetMemberImage(Guid currentMemberId, Guid memberId);

        Task<MemberProfile> GetMemberProfile(Guid currentMemberId, Guid memberId);

        Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId);

        Task<MemberImage> UpdateMemberImage(MemberImage image);

        Task<MemberProfile> UpdateMemberProfile(UpdateMemberProfile profile);
    }
}

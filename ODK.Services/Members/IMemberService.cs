using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Members;

namespace ODK.Services.Members
{
    public interface IMemberService
    {
        Task CreateMember(Guid chapterId, CreateMemberProfile profile);

        Task<IReadOnlyCollection<Member>> GetLatestMembers(Guid currentMemberId, Guid chapterId);

        Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid currentMemberId, Guid memberId);

        Task<MemberProfile> GetMemberProfile(Guid currentMemberId, Guid memberId);

        Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId);

        Task<MemberImage> UpdateMemberImage(Guid id, UpdateMemberImage image);

        Task<MemberProfile> UpdateMemberProfile(Guid id, UpdateMemberProfile profile);
    }
}

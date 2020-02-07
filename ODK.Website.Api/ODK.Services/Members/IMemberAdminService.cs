using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Members;
using ODK.Services.Files;

namespace ODK.Services.Members
{
    public interface IMemberAdminService
    {
        Task DeleteMember(Guid currentMemberId, Guid memberId);

        Task DisableMember(Guid currentMemberId, Guid memberId);

        Task EnableMember(Guid currentMemberId, Guid memberId);

        Task<Member> GetMember(Guid currentMemberId, Guid memberId);

        Task<CsvFile> GetMemberImportFile(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId);

        Task<MemberSubscription> GetMemberSubscription(Guid currentMemberId, Guid memberId);

        Task<IReadOnlyCollection<MemberSubscription>> GetMemberSubscriptions(Guid currentMemberId, Guid chapterId);

        Task ImportMembers(Guid currentMemberId, Guid chapterId, CsvFile file);

        Task<MemberImage> RotateMemberImage(Guid currentMemberId, Guid memberId, int degrees);

        Task<MemberImage> UpdateMemberImage(Guid currentMemberId, Guid memberId, UpdateMemberImage image);

        Task<MemberSubscription> UpdateMemberSubscription(Guid currentMemberId, Guid memberId, UpdateMemberSubscription subscription);
    }
}

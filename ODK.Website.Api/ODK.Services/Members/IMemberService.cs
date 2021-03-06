﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Members;

namespace ODK.Services.Members
{
    public interface IMemberService
    {
        Task ConfirmEmailAddressUpdate(Guid memberId, string confirmationToken);

        Task CreateMember(Guid chapterId, CreateMemberProfile profile);

        Task DeleteMember(Guid memberId);

        Task<VersionedServiceResult<IReadOnlyCollection<Member>>> GetLatestMembers(long? currentVersion, Guid currentMemberId, Guid chapterId);

        Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId, int? size);

        Task<MemberProfile> GetMemberProfile(Guid currentMemberId, Guid memberId);

        Task<VersionedServiceResult<IReadOnlyCollection<Member>>> GetMembers(long? currentVersion, Guid currentMemberId, Guid chapterId);

        Task<MemberSubscription> GetMemberSubscription(Guid memberId);

        Task<MemberSubscription> PurchaseSubscription(Guid memberId, Guid chapterSubscriptionId, string cardToken);

        Task RequestMemberEmailAddressUpdate(Guid memberId, string newEmailAddress);

        Task<MemberImage> RotateMemberImage(Guid memberId, int degrees);

        Task UpdateMemberEmailOptIn(Guid memberId, bool optIn);

        Task<MemberImage> UpdateMemberImage(Guid id, UpdateMemberImage image);

        Task<MemberProfile> UpdateMemberProfile(Guid id, UpdateMemberProfile profile);
    }
}

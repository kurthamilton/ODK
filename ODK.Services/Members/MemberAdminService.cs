using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Exceptions;

namespace ODK.Services.Members;

public class MemberAdminService : OdkAdminServiceBase, IMemberAdminService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IChapterRepository _chapterRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberService _memberService;

    public MemberAdminService(IChapterRepository chapterRepository, IMemberRepository memberRepository,
        IMemberService memberService, ICacheService cacheService, IAuthorizationService authorizationService)
        : base(chapterRepository)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _chapterRepository = chapterRepository;
        _memberRepository = memberRepository;
        _memberService = memberService;
    }

    public async Task DeleteMember(Guid currentMemberId, Guid memberId)
    {
        Member? member = await GetMember(currentMemberId, memberId);
        if (member == null)
        {
            return;
        }

        await _memberRepository.DeleteMemberAsync(member.Id);

        _cacheService.RemoveVersionedCollection<Member>(member.ChapterId);
        _cacheService.RemoveVersionedItem<Member>(memberId);
    }

    public async Task DisableMember(Guid currentMemberId, Guid id)
    {
        Member? member = await GetMember(currentMemberId, id);
        if (member == null)
        {
            return;
        }

        await _memberRepository.DisableMemberAsync(member.Id);
    }

    public async Task EnableMember(Guid currentMemberId, Guid id)
    {
        Member? member = await GetMember(currentMemberId, id);
        if (member == null)
        {
            return;
        }

        await _memberRepository.EnableMemberAsync(member.Id);
    }

    public async Task<Member?> GetMember(Guid currentMemberId, Guid memberId)
    {
        return await GetMember(currentMemberId, memberId, false);
    }
    
    public async Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId, bool requireSuperAdmin = false)
    {
        if (requireSuperAdmin)
        {
            await AssertMemberIsChapterSuperAdmin(currentMemberId, chapterId);
        }
        else
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);
        }

        return await _memberRepository.GetMembersAsync(chapterId, true);
    }

    public async Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, MemberFilter filter)
    {
        var membersTask = GetMembers(currentMemberId, filter.ChapterId);
        var memberSubscriptionsTask = GetMemberSubscriptions(currentMemberId, filter.ChapterId);
        var membershipSettingsTask = _chapterRepository.GetChapterMembershipSettings(filter.ChapterId);

        await Task.WhenAll(membersTask, memberSubscriptionsTask, membershipSettingsTask);

        var members = membersTask.Result;
        var memberSubscriptions = memberSubscriptionsTask.Result;
        var memberSubscriptionsDictionary = memberSubscriptions.ToDictionary(x => x.MemberId);
        var membershipSettings = membershipSettingsTask.Result;

        var filteredMembers = new List<Member>();
        foreach (var member in members)
        {
            if (!memberSubscriptionsDictionary.TryGetValue(member.Id, out var memberSubscription))
            {
                continue;
            }

            if (filter.Types.Contains(memberSubscription.Type))
            {
                filteredMembers.Add(member);
                continue;
            }

            if (membershipSettings == null)
            {
                continue;
            }

            var status = _authorizationService.GetSubscriptionStatus(memberSubscription, membershipSettings);
            if (filter.Statuses.Contains(status))
            {
                filteredMembers.Add(member);
                continue;
            }
        }

        return filteredMembers;
    }

    public async Task<MemberSubscription?> GetMemberSubscription(Guid currentMemberId, Guid memberId)
    {
        Member? member = await GetMember(currentMemberId, memberId);
        if (member == null)
        {
            return null;
        }

        return await _memberRepository.GetMemberSubscriptionAsync(member.Id);
    }

    public async Task<IReadOnlyCollection<MemberSubscription>> GetMemberSubscriptions(Guid currentMemberId, Guid chapterId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        return await _memberRepository.GetMemberSubscriptionsAsync(chapterId);
    }
    
    public async Task RotateMemberImage(Guid currentMemberId, Guid memberId, int degrees)
    {
        Member? member = await GetMember(currentMemberId, memberId);
        if (member == null)
        {
            return;
        }

        await _memberService.RotateMemberImage(member.Id, degrees);

        _cacheService.RemoveVersionedItem<MemberImage>(memberId);
    }
    
    public async Task<ServiceResult> UpdateMemberSubscription(Guid currentMemberId, Guid memberId,
        UpdateMemberSubscription subscription)
    {
        Member? member = await GetMember(currentMemberId, memberId);
        if (member == null)
        {
            return ServiceResult.Failure("Member not found");
        }

        DateTime? expiryDate = subscription.Type == SubscriptionType.Alum ? new DateTime?() : subscription.ExpiryDate;

        MemberSubscription update = new MemberSubscription(member.Id, subscription.Type, expiryDate);

        ServiceResult validationResult = ValidateMemberSubscription(update);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        await _memberRepository.UpdateMemberSubscriptionAsync(update);

        _cacheService.RemoveVersionedItem<MemberSubscription>(memberId);
        _cacheService.RemoveVersionedCollection<Member>(member.ChapterId);

        return ServiceResult.Successful();
    }
    
    private async Task<Member?> GetMember(Guid currentMemberId, Guid id, bool superAdmin)
    {
        Member? member = await _memberRepository.GetMemberAsync(id, true);
        if (member == null)
        {
            throw new OdkNotFoundException();
        }

        if (superAdmin)
        {
            await AssertMemberIsChapterSuperAdmin(currentMemberId, member.ChapterId);
        }
        else
        {
            await AssertMemberIsChapterAdmin(currentMemberId, member.ChapterId);
        }

        return member;
    }
    
    private ServiceResult ValidateMemberSubscription(MemberSubscription subscription)
    {
        if (!Enum.IsDefined(typeof(SubscriptionType), subscription.Type) || subscription.Type == SubscriptionType.None)
        {
            return ServiceResult.Failure("Invalid type");
        }

        if (subscription.Type == SubscriptionType.Alum && subscription.ExpiryDate != null)
        {
            return ServiceResult.Failure("Alum should not have expiry date");
        }

        if (subscription.Type != SubscriptionType.Alum && subscription.ExpiryDate < DateTime.UtcNow.Date)
        {
            return ServiceResult.Failure("Expiry date should not be in the past");
        }

        return ServiceResult.Successful();
    }
}

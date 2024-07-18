using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;

namespace ODK.Services.Members;

public class MemberAdminService : OdkAdminServiceBase, IMemberAdminService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IMemberService _memberService;
    private readonly IUnitOfWork _unitOfWork;

    public MemberAdminService(IUnitOfWork unitOfWork, IMemberService memberService, 
        ICacheService cacheService, IAuthorizationService authorizationService)
        : base(unitOfWork)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _memberService = memberService;
        _unitOfWork = unitOfWork;
    }

    public async Task DeleteMember(Guid currentMemberId, Guid memberId)
    {
        var member = await GetMember(currentMemberId, memberId);
        if (member == null)
        {
            return;
        }

        _unitOfWork.MemberRepository.Delete(member);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedCollection<Member>(member.ChapterId);
        _cacheService.RemoveVersionedItem<Member>(memberId);
    }

    public async Task<Member?> GetMember(Guid currentMemberId, Guid memberId)
    {
        return await GetMember(currentMemberId, memberId, superAdmin: false);
    }
    
    public async Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberCsv(Guid currentMemberId, Guid chapterId)
    {
        var (members, subscriptions) = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.MemberRepository.GetByChapterId(chapterId),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapterId));

        var csv = new List<IReadOnlyCollection<string>>
        {
            new []
            {
                "ID",
                "Email",
                "FirstName",
                "LastName",
                "Joined",
                "Activated",
                "Disabled",
                "EmailOptIn",
                "SubscriptionExpiryDate",
                "SubscriptionType"
            }
        };

        var subscriptionDictionary = subscriptions.ToDictionary(x => x.MemberId);

        foreach (var member in members.OrderBy(x => x.FullName))
        {
            subscriptionDictionary.TryGetValue(member.Id, out var subscription);

            csv.Add(
            [
                member.Id.ToString(),
                member.EmailAddress,
                member.FirstName,
                member.LastName,
                member.CreatedDate.ToString("yyyy-MM-dd"),
                member.Activated ? "Y" : "",
                member.Disabled ? "Y" : "",
                member.EmailOptIn ? "Y" : "",
                subscription?.ExpiryDate?.ToString("yyyy-MM-dd") ?? "",
                subscription?.Type.ToString() ?? ""
            ]);
        }

        return csv;
    }

    public async Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, MemberFilter filter)
    {
        var (members, memberSubscriptions, membershipSettings) = await GetChapterAdminRestrictedContent(currentMemberId, filter.ChapterId,
            x => x.MemberRepository.GetByChapterId(filter.ChapterId, true),
            x => x.MemberSubscriptionRepository.GetByChapterId(filter.ChapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(filter.ChapterId));

        var memberSubscriptionsDictionary = memberSubscriptions.ToDictionary(x => x.MemberId);

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

    public async Task<MembersDto> GetMembersDto(Guid currentMemberId, Guid chapterId)
    {
        var (members, subscriptions) = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.MemberRepository.GetByChapterId(chapterId),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapterId));

        return new MembersDto
        {
            Members = members,
            Subscriptions = subscriptions
        };
    }

    public async Task<MemberSubscription?> GetMemberSubscription(Guid currentMemberId, Guid memberId)
    {
        var (member, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId));

        await AssertMemberIsChapterAdmin(currentMemberId, member.ChapterId);

        return memberSubscription;
    }

    public async Task<IReadOnlyCollection<MemberSubscription>> GetMemberSubscriptions(Guid currentMemberId, Guid chapterId)
    {
        var (chapterAdminMembers, currentMember, subscriptions) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapterId));

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return subscriptions;
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
        UpdateMemberSubscription update)
    {
        var (member, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId));

        var expiryDate = update.Type == SubscriptionType.Alum 
            ? new DateTime?() 
            : update.ExpiryDate;

        if (memberSubscription == null)
        {
            memberSubscription = new MemberSubscription
            {
                ExpiryDate = expiryDate,                
                Type = update.Type
            };            
        }

        var validationResult = ValidateMemberSubscription(memberSubscription);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (memberSubscription.MemberId == Guid.Empty)
        {
            memberSubscription.MemberId = memberId;
            _unitOfWork.MemberSubscriptionRepository.Add(memberSubscription);
        }
        else
        {
            _unitOfWork.MemberSubscriptionRepository.Update(memberSubscription);
        }

        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedItem<MemberSubscription>(memberId);
        _cacheService.RemoveVersionedCollection<Member>(member.ChapterId);

        return ServiceResult.Successful();
    }
    
    private async Task<Member?> GetMember(Guid currentMemberId, Guid id, bool superAdmin)
    {
        var member = await _unitOfWork.MemberRepository.GetByIdOrDefault(id, true).RunAsync();
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

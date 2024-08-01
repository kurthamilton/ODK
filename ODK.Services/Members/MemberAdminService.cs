using ODK.Core.Chapters;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Exceptions;

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
        var (chapterAdminMembers, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberRepository.GetById(memberId));

        AssertCurrentAdminMemberCanSeeMember(currentMemberId, chapterAdminMembers, member);

        _unitOfWork.MemberRepository.Delete(member);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Member> GetMember(Guid currentMemberId, Guid memberId)
    {
        var (chapterAdminMembers, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberRepository.GetById(memberId));

        AssertCurrentAdminMemberCanSeeMember(currentMemberId, chapterAdminMembers, member);

        return member;
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
                member.CreatedUtc.ToString("yyyy-MM-dd"),
                member.Activated ? "Y" : "",
                member.Disabled ? "Y" : "",
                member.EmailOptIn ? "Y" : "",
                subscription?.ExpiresUtc?.ToString("yyyy-MM-dd") ?? "",
                subscription?.Type.ToString() ?? ""
            ]);
        }

        return csv;
    }

    public Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId) => GetMembers(currentMemberId,
        new MemberFilter
        {
            ChapterId = chapterId,
            Statuses = Enum.GetValues<SubscriptionStatus>().ToList(),
            Types = Enum.GetValues<SubscriptionType>().ToList()
        });

    public async Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, MemberFilter filter)
    {
        var (members, memberSubscriptions, membershipSettings) = await GetChapterAdminRestrictedContent(currentMemberId, filter.ChapterId,
            x => x.MemberRepository.GetAllByChapterId(filter.ChapterId),
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
            x => x.MemberRepository.GetAllByChapterId(chapterId),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapterId));

        return new MembersDto
        {
            Members = members,
            Subscriptions = subscriptions
        };
    }

    public async Task<MemberSubscription?> GetMemberSubscription(Guid currentMemberId, Guid chapterId, Guid memberId)
    {
        var (chapterAdminMembers, currentMember, member, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, chapterId));

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

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
        var (chapterAdminMembers, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberRepository.GetById(memberId));
        
        await _memberService.RotateMemberImage(member.Id, degrees);

        _cacheService.RemoveVersionedItem<MemberImage>(memberId);
    }
    
    public async Task SendActivationEmail(Guid currentMemberId, Guid chapterId, Guid memberId)
    {
        var (chapter, currentMember, chapterAdminMembers, member, memberActivationToken) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberActivationTokenRepository.GetByMemberId(memberId));        

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        if (!member.IsMemberOf(chapterId) || memberActivationToken == null)
        {
            throw new OdkNotFoundException();
        }
        
        await _memberService.SendActivationEmailAsync(chapter, member, memberActivationToken.ActivationToken);
    }

    public async Task<ServiceResult> UpdateMemberSubscription(Guid currentMemberId, Guid chapterId, Guid memberId,
        UpdateMemberSubscription model)
    {
        var (chapterAdminMembers, currentMember, member, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, chapterId));

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        var expiryDate = model.Type == SubscriptionType.Alum 
            ? new DateTime?() 
            : model.ExpiryDate;

        if (memberSubscription == null)
        {
            memberSubscription = new MemberSubscription();
        }

        memberSubscription.ExpiresUtc = expiryDate;
        memberSubscription.Type = model.Type;

        var validationResult = ValidateMemberSubscription(memberSubscription);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (memberSubscription.MemberId == default)
        {
            memberSubscription.ChapterId = chapterId;
            memberSubscription.MemberId = memberId;
            _unitOfWork.MemberSubscriptionRepository.Add(memberSubscription);
        }
        else
        {
            _unitOfWork.MemberSubscriptionRepository.Update(memberSubscription);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    private void AssertCurrentAdminMemberCanSeeMember(
        Guid currentMemberId,
        IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers,
        Member member)
    {
        if (!CurrentMemberCanSeeMember(currentMemberId, chapterAdminMembers, member))
        {
            throw new OdkNotAuthorizedException();
        }
    }

    private bool CurrentMemberCanSeeMember(
        Guid currentMemberId, 
        IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers, 
        Member member)
    {
        var chapterAdminMember = chapterAdminMembers
            .FirstOrDefault(x => x.MemberId == currentMemberId);
        return chapterAdminMember != null && member.SharesChapterWith(chapterAdminMember.Member);
    }

    private ServiceResult ValidateMemberSubscription(MemberSubscription subscription)
    {
        if (!Enum.IsDefined(typeof(SubscriptionType), subscription.Type) || subscription.Type == SubscriptionType.None)
        {
            return ServiceResult.Failure("Invalid type");
        }

        if (subscription.Type == SubscriptionType.Alum && subscription.ExpiresUtc != null)
        {
            return ServiceResult.Failure("Alum should not have expiry date");
        }

        if (subscription.Type != SubscriptionType.Alum && subscription.ExpiresUtc < DateTime.UtcNow.Date)
        {
            return ServiceResult.Failure("Expiry date should not be in the past");
        }

        return ServiceResult.Successful();
    }
}

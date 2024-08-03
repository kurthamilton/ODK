using ODK.Core.Chapters;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Emails;

namespace ODK.Services.Members;

public class MemberAdminService : OdkAdminServiceBase, IMemberAdminService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IEmailService _emailService;
    private readonly IMemberImageService _memberImageService;
    private readonly IMemberService _memberService;
    private readonly MemberAdminServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public MemberAdminService(IUnitOfWork unitOfWork, IMemberService memberService, 
        ICacheService cacheService, IAuthorizationService authorizationService,
        MemberAdminServiceSettings settings, IMemberImageService memberImageService,
        IEmailService emailService)
        : base(unitOfWork)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _emailService = emailService;
        _memberImageService = memberImageService;
        _memberService = memberService;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task DeleteMember(AdminServiceRequest request, Guid memberId)
    {
        var member = await GetMember(request, memberId);

        _unitOfWork.MemberRepository.Delete(member);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Member> GetMember(AdminServiceRequest request, Guid memberId)
    {
        var member = await GetChapterAdminRestrictedContent(request,
            x => x.MemberRepository.GetById(memberId));

        AssertMemberIsInChapter(member, request);

        return member;
    }
    
    public async Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberCsv(AdminServiceRequest request)
    {
        var (members, subscriptions) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberRepository.GetByChapterId(request.ChapterId),
            x => x.MemberSubscriptionRepository.GetByChapterId(request.ChapterId));

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
                member.MemberChapter(request.ChapterId).CreatedUtc.ToString("yyyy-MM-dd"),
                member.Activated ? "Y" : "",
                member.Disabled ? "Y" : "",
                member.EmailOptIn ? "Y" : "",
                subscription?.ExpiresUtc?.ToString("yyyy-MM-dd") ?? "",
                subscription?.Type.ToString() ?? ""
            ]);
        }

        return csv;
    }

    public Task<IReadOnlyCollection<Member>> GetMembers(AdminServiceRequest request) => GetMembers(request,
        new MemberFilter
        {
            Statuses = Enum.GetValues<SubscriptionStatus>().ToList(),
            Types = Enum.GetValues<SubscriptionType>().ToList()
        });

    public async Task<IReadOnlyCollection<Member>> GetMembers(AdminServiceRequest request, MemberFilter filter)
    {
        var (members, memberSubscriptions, membershipSettings) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberRepository.GetAllByChapterId(request.ChapterId),
            x => x.MemberSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(request.ChapterId));        

        return FilterMembers(members, memberSubscriptions, membershipSettings, filter)
            .ToArray();
    }

    public async Task<MembersDto> GetMembersDto(AdminServiceRequest request)
    {
        var (members, subscriptions) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberRepository.GetAllByChapterId(request.ChapterId),
            x => x.MemberSubscriptionRepository.GetByChapterId(request.ChapterId));

        return new MembersDto
        {
            Members = members,
            Subscriptions = subscriptions
        };
    }

    public async Task<MemberSubscription?> GetMemberSubscription(AdminServiceRequest request, Guid memberId)
    {
        var (member, memberSubscription) = await GetChapterAdminRestrictedContent(request,            
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, request.ChapterId));

        AssertMemberIsInChapter(member, request);

        return memberSubscription;
    }

    public async Task RotateMemberImage(AdminServiceRequest request, Guid memberId)
    {
        var member = await GetMember(request, memberId);

        await _memberService.RotateMemberImage(member.Id);
    }
    
    public async Task SendActivationEmail(AdminServiceRequest request, Guid memberId)
    {
        var (chapter, member, memberActivationToken) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberActivationTokenRepository.GetByMemberId(memberId));

        AssertMemberIsInChapter(member, request);
        AssertExists(memberActivationToken);
        
        await _memberService.SendActivationEmailAsync(chapter, member, memberActivationToken.ActivationToken);
    }

    public async Task<ServiceResult> SendBulkEmail(AdminServiceRequest request, MemberFilter filter, string subject, string body)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMember, chapter, members, memberSubscriptions, membershipSettings) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId, chapterId),
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberRepository.GetByChapterId(chapterId),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId));

        var filteredMembers = FilterMembers(members, memberSubscriptions, membershipSettings, filter);

        await _emailService.SendBulkEmail(chapterAdminMember, chapter, filteredMembers, subject, body);

        return ServiceResult.Successful($"Bulk email sent to {members.Count} members");
    }

    public async Task<ServiceResult> SendMemberEmail(AdminServiceRequest request, Guid memberId, string subject, string body)
    {
        var (chapter, chapterAdminMember, member) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByMemberId(request.CurrentMemberId, request.ChapterId),
            x => x.MemberRepository.GetById(memberId));

        return await _emailService.SendMemberEmail(chapter, chapterAdminMember, member, subject, body);
    }

    public async Task SetMemberVisibility(AdminServiceRequest request, Guid memberId, bool visible)
    {
        var chapterId = request.ChapterId;

        var member = await GetSuperAdminRestrictedContent(request.CurrentMemberId,
            x => x.MemberRepository.GetById(memberId));

        AssertMemberIsInChapter(member, request);

        var privacySettings = member.PrivacySettings
            .FirstOrDefault(x => x.ChapterId == chapterId);

        if (privacySettings == null)
        {
            privacySettings = new MemberChapterPrivacySettings();
        }

        privacySettings.HideProfile = !visible;

        if (privacySettings.MemberId == Guid.Empty)
        {
            privacySettings.ChapterId = chapterId;
            privacySettings.MemberId = member.Id;
            _unitOfWork.MemberChapterPrivacySettingsRepository.Add(privacySettings);
        }
        else
        {
            _unitOfWork.MemberChapterPrivacySettingsRepository.Update(privacySettings);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> UpdateMemberSubscription(AdminServiceRequest request, Guid memberId,
        UpdateMemberSubscription model)
    {
        var chapterId = request.ChapterId;

        var (member, memberSubscription) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, chapterId));

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

    private static void AssertMemberIsInChapter(Member member, AdminServiceRequest request)
    {
        if (!member.IsMemberOf(request.ChapterId))
        {
            throw new OdkNotFoundException();
        }
    }

    private IEnumerable<Member> FilterMembers(
        IEnumerable<Member> members, 
        IEnumerable<MemberSubscription> memberSubscriptions,
        ChapterMembershipSettings? membershipSettings,
        MemberFilter filter)
    {
        var memberSubscriptionsDictionary = memberSubscriptions.ToDictionary(x => x.MemberId);

        foreach (var member in members)
        {
            if (!memberSubscriptionsDictionary.TryGetValue(member.Id, out var memberSubscription))
            {
                continue;
            }

            if (filter.Types.Contains(memberSubscription.Type))
            {
                yield return member;
                continue;
            }

            if (membershipSettings == null)
            {
                continue;
            }

            var status = _authorizationService.GetSubscriptionStatus(memberSubscription, membershipSettings);
            if (filter.Statuses.Contains(status))
            {
                yield return member;
                continue;
            }
        }
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

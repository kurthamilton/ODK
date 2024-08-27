using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Utils;
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
    private readonly IMemberEmailService _memberEmailService;
    private readonly IMemberImageService _memberImageService;
    private readonly IMemberService _memberService;
    private readonly MemberAdminServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public MemberAdminService(IUnitOfWork unitOfWork, IMemberService memberService, 
        ICacheService cacheService, IAuthorizationService authorizationService,
        MemberAdminServiceSettings settings, IMemberImageService memberImageService,
        IEmailService emailService, IMemberEmailService memberEmailService)
        : base(unitOfWork)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _emailService = emailService;
        _memberEmailService = memberEmailService;
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
    
    public async Task<MemberAvatar?> GetMemberAvatar(AdminServiceRequest request, Guid memberId)
    {
        var (member, memberAvatar) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberAvatarRepository.GetByMemberId(memberId));

        OdkAssertions.MemberOf(member, request.ChapterId);

        return memberAvatar;
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
                member.EmailOptIn ? "Y" : "",
                subscription?.ExpiresUtc?.ToString("yyyy-MM-dd") ?? "",
                subscription?.Type.ToString() ?? ""
            ]);
        }

        return csv;
    }

    public async Task<MemberImage?> GetMemberImage(AdminServiceRequest request, Guid memberId)
    {
        var (member, memberImage) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberImageRepository.GetByMemberId(memberId));

        OdkAssertions.MemberOf(member, request.ChapterId);

        return memberImage;
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
        OdkAssertions.Exists(memberActivationToken);
        
        await _memberEmailService.SendActivationEmail(chapter, member, memberActivationToken.ActivationToken);
    }

    public async Task<ServiceResult> SendBulkEmail(AdminServiceRequest request, MemberFilter filter, string subject, string body)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMember, chapter, members, memberSubscriptions, membershipSettings, subscription) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId, chapterId),
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberRepository.GetByChapterId(chapterId),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId));

        var authorized = _authorizationService.ChapterHasAccess(subscription, SiteFeatureType.SendMemberEmails);
        if (!authorized)
        {
            return ServiceResult.Unauthorized(SiteFeatureType.SendMemberEmails);
        }

        var filteredMembers = FilterMembers(members, memberSubscriptions, membershipSettings, filter);

        await _emailService.SendBulkEmail(chapterAdminMember, chapter, filteredMembers, subject, body);

        return ServiceResult.Successful($"Bulk email sent to {members.Count} members");
    }

    public async Task<ServiceResult> SendMemberEmail(AdminServiceRequest request, Guid memberId, string subject, string body)
    {
        var (chapter, chapterAdminMember, member, subscription) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByMemberId(request.CurrentMemberId, request.ChapterId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId));

        var authorized = _authorizationService.ChapterHasAccess(subscription, SiteFeatureType.SendMemberEmails);
        if (!authorized)
        {
            return ServiceResult.Unauthorized(SiteFeatureType.SendMemberEmails);
        }

        return await _emailService.SendMemberEmail(chapter, chapterAdminMember, member.ToEmailAddressee(), subject, body);
    }

    public async Task SendMemberSubscriptionReminderEmails()
    {
        var chapters = await _unitOfWork.ChapterRepository.GetAll().RunAsync();
        foreach (var chapter in chapters)
        {
            var (members, memberSubscriptions, membershipSettings) = await _unitOfWork.RunAsync(
                x => x.MemberRepository.GetByChapterId(chapter.Id),
                x => x.MemberSubscriptionRepository.GetByChapterId(chapter.Id),
                x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id));

            if (membershipSettings == null || !membershipSettings.Enabled)
            {
                continue;
            }

            var memberSubscriptionDictionary = memberSubscriptions.ToDictionary(x => x.MemberId);            
            foreach (var member in members)
            {
                if (!memberSubscriptionDictionary.TryGetValue(member.Id, out var memberSubscription))
                {
                    memberSubscription = new MemberSubscription
                    {
                        ChapterId = chapter.Id,
                        MemberId = member.Id,
                        Type = SubscriptionType.Trial
                    };
                    _unitOfWork.MemberSubscriptionRepository.Add(memberSubscription);
                    continue;
                }

                if (memberSubscription.ExpiresUtc == null)
                {
                    continue;
                }

                var now = DateTime.UtcNow;
                var expires = memberSubscription.ExpiresUtc.Value;
                if (expires > now.AddDays(7))
                {
                    // no need for reminder
                    continue;
                }

                var expiring = expires > now;
                var expired = !expiring;
                if (expiring && memberSubscription.ReminderEmailSentUtc != null)
                {
                    // membership expiring - reminder already sent
                    continue;
                }

                if (expired && memberSubscription.ReminderEmailSentUtc > memberSubscription.ExpiresUtc)
                {
                    // membership expired - reminder already sent
                    continue;
                }

                if (expired && expires < now.AddDays(-7))
                {
                    // membership expired more than 7 days ago - no need for reminder
                    continue;
                }

                var disabledDate = expires
                    .AddDays(membershipSettings.MembershipDisabledAfterDaysExpired);

                var properties = new Dictionary<string, string>
                {
                    { "chapter.name", chapter.Name },
                    { "member.firstName", member.FirstName },
                    { "subscription.expiryDate", expires.ToFriendlyDateString(chapter.TimeZone) },
                    { "subscription.disabledDate", disabledDate.ToFriendlyDateString(chapter.TimeZone) }
                };

                var emailType = expiring
                    ? memberSubscription.Type switch
                    {
                        SubscriptionType.Trial => EmailType.TrialExpiring,
                        _ => EmailType.SubscriptionExpiring
                    }
                    : memberSubscription.Type switch
                    {
                        SubscriptionType.Trial => EmailType.TrialExpired,
                        _ => EmailType.SubscriptionExpired
                    };

                await _emailService.SendEmail(chapter, member.ToEmailAddressee(), emailType, properties);

                memberSubscription.ReminderEmailSentUtc = now;
                _unitOfWork.MemberSubscriptionRepository.Update(memberSubscription);
                await _unitOfWork.SaveChangesAsync();
            }
        }
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
            _unitOfWork.MemberPrivacySettingsRepository.Add(privacySettings);
        }
        else
        {
            _unitOfWork.MemberPrivacySettingsRepository.Update(privacySettings);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> UpdateMemberImage(AdminServiceRequest request, Guid id, 
        UpdateMemberImage? model, MemberImageCropInfo cropInfo)
    {
        var (member, image, avatar) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberRepository.GetById(id),
            x => x.MemberImageRepository.GetByMemberId(id),
            x => x.MemberAvatarRepository.GetByMemberId(id));

        OdkAssertions.MemberOf(member, request.ChapterId);

        if (image == null)
        {
            image = new MemberImage();
        }

        if (avatar == null)
        {
            avatar = new MemberAvatar();
        }

        var result = _memberImageService.ProcessMemberImage(image, avatar, model, cropInfo);
        if (!result.Success)
        {
            return result;
        }

        if (image.MemberId == Guid.Empty)
        {
            image.MemberId = member.Id;
            _unitOfWork.MemberImageRepository.Add(image);
        }
        else
        {
            _unitOfWork.MemberImageRepository.Update(image);
        }

        if (avatar.MemberId == Guid.Empty)
        {
            avatar.MemberId = member.Id;
            _unitOfWork.MemberAvatarRepository.Add(avatar);
        }
        else
        {
            _unitOfWork.MemberAvatarRepository.Update(avatar);
        }

        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedItem<MemberImage>(id);
        _cacheService.RemoveVersionedItem<MemberAvatar>(id);

        return ServiceResult.Successful("Picture updated");
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
        => OdkAssertions.MeetsCondition(member, x => x.IsMemberOf(request.ChapterId));

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

            var status = _authorizationService.GetSubscriptionStatus(member, memberSubscription, membershipSettings);
            if (filter.Types.Contains(memberSubscription.Type) &&
                filter.Statuses.Contains(status))
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

        return ServiceResult.Successful();
    }    
}

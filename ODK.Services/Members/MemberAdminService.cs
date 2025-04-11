using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Events.ViewModels;
using ODK.Services.Members.Models;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public class MemberAdminService : OdkAdminServiceBase, IMemberAdminService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IMemberImageService _memberImageService;
    private readonly IMemberService _memberService;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    public MemberAdminService(
        IUnitOfWork unitOfWork, 
        IMemberService memberService, 
        ICacheService cacheService, 
        IAuthorizationService authorizationService,
        IMemberImageService memberImageService,
        IMemberEmailService memberEmailService,
        IPlatformProvider platformProvider)
        : base(unitOfWork)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _memberEmailService = memberEmailService;
        _memberImageService = memberImageService;
        _memberService = memberService;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ApproveMember(AdminServiceRequest request, Guid memberId)
    {
        var (chapter, member) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberRepository.GetById(memberId));

        var memberChapter = member.MemberChapter(request.ChapterId);

        OdkAssertions.MemberOf(member, request.ChapterId);
        OdkAssertions.Exists(memberChapter);

        memberChapter.Approved = true;

        _unitOfWork.MemberChapterRepository.Update(memberChapter);
        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendMemberApprovedEmail(chapter, member);

        return ServiceResult.Successful();
    }

    public async Task<AdminMemberAdminPageViewModel> GetAdminMemberViewModel(AdminServiceRequest request, Guid memberId)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, adminMembers) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(request.ChapterId));

        var adminMember = adminMembers.FirstOrDefault(x => x.MemberId == memberId);
        OdkAssertions.Exists(adminMember);

        AssertMemberIsChapterAdmin(adminMember.Member, request.ChapterId, adminMembers);

        return new AdminMemberAdminPageViewModel
        {
            AdminMember = adminMember,
            Chapter = chapter,
            Platform = platform
        };
    }

    public async Task<AdminMembersAdminPageViewModel> GetAdminMembersAdminPageViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, adminMembers, members, ownerSubscription) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(request.ChapterId),
            x => x.MemberRepository.GetByChapterId(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId));

        return new AdminMembersAdminPageViewModel
        {
            AdminMembers = adminMembers,
            Chapter = chapter,
            Members = members,
            OwnerSubscription = ownerSubscription,
            Platform = platform
        };
    }

    public async Task<BulkEmailAdminPageViewModel> GetBulkEmailViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

        var chapter = await _unitOfWork.ChapterRepository.GetById(request.ChapterId).Run();

        return new BulkEmailAdminPageViewModel
        {
            Chapter = chapter,
            Platform = platform
        };
    }

    public async Task<Member> GetMember(AdminServiceRequest request, Guid memberId)
    {
        var member = await GetChapterAdminRestrictedContent(request,
            x => x.MemberRepository.GetById(memberId));

        AssertMemberIsInChapter(member, request);

        return member;
    }
    
    public async Task<MemberApprovalsAdminPageViewModel> GetMemberApprovalsViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, members, membershipSettings) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberRepository.GetAllByChapterId(request.ChapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(request.ChapterId));

        return new MemberApprovalsAdminPageViewModel
        {
            Chapter = chapter,
            MembershipSettings = membershipSettings,
            Pending = members
                .Where(x => x.MemberChapter(request.ChapterId)?.Approved == false)
                .ToArray(),
            Platform = platform
        };
    }

    public async Task<MemberAvatar?> GetMemberAvatar(AdminServiceRequest request, Guid memberId)
    {
        var (member, memberAvatar) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberAvatarRepository.GetByMemberId(memberId));

        OdkAssertions.MemberOf(member, request.ChapterId);

        return memberAvatar;
    }

    public async Task<MemberConversationsAdminPageViewModel> GetMemberConversationsViewModel(AdminServiceRequest request, 
        Guid memberId)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, member, conversations, ownerSubscription) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterConversationRepository.GetDtosByMemberId(memberId, request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId));

        OdkAssertions.MemberOf(member, chapter.Id);

        return new MemberConversationsAdminPageViewModel
        {
            Chapter = chapter,
            Conversations = conversations,
            Member = member,
            OwnerSubscription = ownerSubscription,
            Platform = platform
        };
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
                "FirstName",
                "LastName",
                "Joined",
                "Activated",
                "SubscriptionExpiryDate",
                "SubscriptionType"
            }
        };

        var subscriptionDictionary = subscriptions
            .ToDictionary(x => x.MemberChapter.MemberId);

        foreach (var member in members.OrderBy(x => x.FullName))
        {
            subscriptionDictionary.TryGetValue(member.Id, out var subscription);

            csv.Add(
            [
                member.Id.ToString(),
                member.FirstName,
                member.LastName,
                member.MemberChapter(request.ChapterId)?.CreatedUtc.ToString("yyyy-MM-dd") ?? "",
                member.Activated ? "Y" : "",
                subscription?.ExpiresUtc?.ToString("yyyy-MM-dd") ?? "",
                subscription?.Type.ToString() ?? ""
            ]);
        }

        return csv;
    }

    public async Task<MemberDeleteAdminPageViewModel> GetMemberDeleteViewModel(AdminServiceRequest request, Guid memberId)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, member, subscription) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, request.ChapterId));

        return new MemberDeleteAdminPageViewModel
        {
            Chapter = chapter,
            Member = member,
            Platform = platform,
            MemberSubscription = subscription
        };
    }

    public async Task<MemberEventsAdminPageViewModel> GetMemberEventsViewModel(AdminServiceRequest request, Guid memberId)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, member, events, venues, memberResponses, invites) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberRepository.GetById(memberId),
            x => x.EventRepository.GetByChapterId(request.ChapterId),
            x => x.VenueRepository.GetByChapterId(request.ChapterId),
            x => x.EventResponseRepository.GetAllByMemberId(memberId, request.ChapterId),
            x => x.EventInviteRepository.GetAllByMemberId(memberId, request.ChapterId));

        OdkAssertions.MemberOf(member, chapter.Id);

        var responseViewModels = new List<EventResponseViewModel>();

        var inviteDictionary = invites.ToDictionary(x => x.EventId);
        var responseDictionary = memberResponses.ToDictionary(x => x.EventId);
        var venueDictionary = venues.ToDictionary(x => x.Id);

        foreach (var @event in events)
        {
            inviteDictionary.TryGetValue(@event.Id, out var invite);
            responseDictionary.TryGetValue(@event.Id, out var response);
            if (invite == null && response == null)
            {
                continue;
            }

            venueDictionary.TryGetValue(@event.VenueId, out var venue);

            var responseViewModel = new EventResponseViewModel(
                @event: @event, 
                venue: venue, 
                response: response?.Type ?? EventResponseType.None, 
                invited: invite != null,
                responseSummary: null);
            responseViewModels.Add(responseViewModel);
        }

        return new MemberEventsAdminPageViewModel
        {
            Chapter = chapter,
            Member = member,
            Platform = platform,
            Responses = responseViewModels
        };
    }

    public async Task<MemberImageAdminPageViewModel> GetMemberImageViewModel(AdminServiceRequest request, Guid memberId)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, member, image, avatar) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberImageRepository.GetByMemberId(memberId),
            x => x.MemberAvatarRepository.GetByMemberId(memberId));

        OdkAssertions.MemberOf(member, request.ChapterId);

        return new MemberImageAdminPageViewModel
        {
            Avatar = avatar,
            Chapter = chapter,
            Image = image,
            Member = member,
            Platform = platform
        };
    }

    public async Task<MemberPaymentsAdminPageViewModel> GetMemberPaymentsViewModel(AdminServiceRequest request, Guid memberId)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, member, chapterPaymentSettings, payments) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.PaymentRepository.GetMemberChapterPayments(memberId, request.ChapterId));

        return new MemberPaymentsAdminPageViewModel
        {
            Chapter = chapter,
            ChapterPaymentSettings = chapterPaymentSettings,
            Member = member,
            Payments = payments,
            Platform = platform,
        };
    }

    public async Task<SubscriptionCreateAdminPageViewModel> GetMemberSubscriptionCreateViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, ownerSubscription, paymentSettings) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId));

        return new SubscriptionCreateAdminPageViewModel
        {
            Chapter = chapter,
            OwnerSubscription = ownerSubscription,
            PaymentSettings = paymentSettings,
            Platform = platform
        };
    }

    public async Task<SubscriptionsAdminPageViewModel> GetMemberSubscriptionsViewModel(
        AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (
            chapter, 
            ownerSubscription, 
            chapterAdminMembers, 
            currentMember, 
            chapterSubscriptions, 
            chapterPaymentSettings, 
            membershipSettings,
            sitePaymentSettings
        ) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId),
            x => x.SitePaymentSettingsRepository.GetActive());

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        chapterSubscriptions = chapterSubscriptions
            .Where(x => chapterPaymentSettings.UseSitePaymentProvider
                ? x.SitePaymentSettingId == sitePaymentSettings.Id
                : x.SitePaymentSettingId == null)
            .ToArray();

        return new SubscriptionsAdminPageViewModel
        {
            Chapter = chapter,
            ChapterSubscriptions = chapterSubscriptions,
            Currency = chapterPaymentSettings?.Currency,
            ExternalSubscription = null,
            MembershipSettings = membershipSettings ?? new(),
            MemberSubscription = null,
            OwnerSubscription = ownerSubscription,
            PaymentSettings = chapterPaymentSettings?.UseSitePaymentProvider == true
                ? sitePaymentSettings
                : chapterPaymentSettings,
            Platform = platform
        };
    }

    public async Task<SubscriptionAdminPageViewModel> GetMemberSubscriptionViewModel(AdminServiceRequest request, Guid subscriptionId)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, ownerSubscription, paymentSettings, subscription) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterSubscriptionRepository.GetById(subscriptionId));

        OdkAssertions.BelongsToChapter(subscription, request.ChapterId);

        return new SubscriptionAdminPageViewModel
        {
            Chapter = chapter,
            OwnerSubscription = ownerSubscription,
            PaymentSettings = paymentSettings,
            Platform = platform,
            Subscription = subscription
        };
    }

    public async Task<MembersAdminPageViewModel> GetMembersViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, membershipSettings, members, memberEmailPreferences, subscriptions) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.MemberRepository.GetAllByChapterId(request.ChapterId),
            x => x.MemberEmailPreferenceRepository.GetByChapterId(request.ChapterId, MemberEmailPreferenceType.Events),
            x => x.MemberSubscriptionRepository.GetByChapterId(request.ChapterId));

        return new MembersAdminPageViewModel
        {
            Chapter = chapter,
            MemberEventEmailPreferences = memberEmailPreferences,
            Members = members,
            MembershipSettings = membershipSettings,
            Platform = platform,
            Subscriptions = subscriptions
        };
    }

    public async Task<MemberAdminPageViewModel> GetMemberViewModel(AdminServiceRequest request, Guid memberId)
    {
        var platform = _platformProvider.GetPlatform();
        
        var (chapter, member, subscription, notifications) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, request.ChapterId),
            x => x.NotificationRepository.GetUnreadByMemberId(request.CurrentMemberId, NotificationType.NewMember, memberId));

        OdkAssertions.MemberOf(member, chapter.Id);

        if (notifications.Count > 0)
        {
            _unitOfWork.NotificationRepository.MarkAsRead(notifications);
            await _unitOfWork.SaveChangesAsync();
        }

        return new MemberAdminPageViewModel
        {
            Chapter = chapter,
            Member = member,
            Platform = platform,
            Subscription = subscription
        };
    }

    public async Task<ServiceResult> RemoveMemberFromChapter(AdminServiceRequest request, Guid memberId, string? reason)
    {
        var (chapter, subscription) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, request.ChapterId));

        if (subscription?.Type.IsPaid() == true && subscription?.IsExpired() == false)
        {
            return ServiceResult.Failure("You cannot remove members with an active paid subscription");
        }

        var result = await _memberService.DeleteMemberChapterData(memberId, chapter.Id);
        var (member, _) = result.Value;
        if (member == null)
        {
            return result;
        }

        await _memberEmailService.SendMemberDeleteEmail(chapter, member, reason);

        return ServiceResult.Successful();
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

        var (
            chapter, 
            members, 
            memberEmailPreferences, 
            memberSubscriptions, 
            membershipSettings, 
            subscription) 
        = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberRepository.GetByChapterId(chapterId),
            x => x.MemberEmailPreferenceRepository.GetByChapterId(chapterId, MemberEmailPreferenceType.ChapterMessages),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId));

        var authorized = _authorizationService.ChapterHasAccess(subscription, SiteFeatureType.SendMemberEmails);
        if (!authorized)
        {
            return ServiceResult.Unauthorized(SiteFeatureType.SendMemberEmails);
        }

        var filteredMembers = FilterMembers(members, memberSubscriptions, membershipSettings, filter)
            .ToArray();

        var optOutMemberIds = memberEmailPreferences
            .Where(x => x.Type == MemberEmailPreferenceType.ChapterMessages && x.Disabled == true)
            .Select(x => x.MemberId)
            .ToHashSet();

        filteredMembers = filteredMembers
            .Where(x => !optOutMemberIds.Contains(x.Id))
            .ToArray();

        await _memberEmailService.SendBulkEmail(chapter, filteredMembers, subject, body);

        return ServiceResult.Successful($"Bulk email sent to {filteredMembers.Length} members");
    }

    public async Task SendMemberSubscriptionReminderEmails()
    {
        var chapters = await _unitOfWork.ChapterRepository.GetAll().Run();
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

            var memberSubscriptionDictionary = memberSubscriptions
                .ToDictionary(x => x.MemberChapter.MemberId);            
            foreach (var member in members)
            {
                var memberChapter = member.MemberChapter(chapter.Id);
                if (memberChapter  == null)
                {
                    continue;
                }

                if (!memberSubscriptionDictionary.TryGetValue(member.Id, out var memberSubscription))
                {
                    memberSubscription = new MemberSubscription
                    {
                        MemberChapterId = memberChapter.Id,
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

                await _memberEmailService.SendMemberChapterSubscriptionExpiringEmail(
                    chapter, 
                    member, 
                    memberSubscription,
                    expires: expires,
                    disabledDate: disabledDate);

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

        privacySettings ??= new MemberChapterPrivacySettings();

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
        UpdateMemberImage model)
    {
        var (member, image, avatar) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberRepository.GetById(id),
            x => x.MemberImageRepository.GetByMemberId(id),
            x => x.MemberAvatarRepository.GetByMemberId(id));

        OdkAssertions.MemberOf(member, request.ChapterId);

        image ??= new MemberImage();

        avatar ??= new MemberAvatar();

        var result = _memberImageService.UpdateMemberImage(image, avatar, model.ImageData);
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

        var memberChapter = member.MemberChapter(chapterId);
        if (memberChapter == null)
        {
            return ServiceResult.Failure("Member chapter not found");
        }

        memberSubscription ??= new MemberSubscription();

        memberSubscription.ExpiresUtc = model.ExpiryDate;
        memberSubscription.Type = model.Type;

        var validationResult = ValidateMemberSubscription(memberSubscription);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (memberSubscription.MemberChapterId == default)
        {
            memberSubscription.MemberChapterId = memberChapter.Id;
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
        var memberSubscriptionsDictionary = memberSubscriptions
            .ToDictionary(x => x.MemberChapter.MemberId);

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

        return ServiceResult.Successful();
    }    
}

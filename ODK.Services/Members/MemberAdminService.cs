using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Events.ViewModels;
using ODK.Services.Exceptions;
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
    private readonly IUnitOfWork _unitOfWork;

    public MemberAdminService(
        IUnitOfWork unitOfWork,
        IMemberService memberService,
        ICacheService cacheService,
        IAuthorizationService authorizationService,
        IMemberImageService memberImageService,
        IMemberEmailService memberEmailService)
        : base(unitOfWork)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _memberEmailService = memberEmailService;
        _memberImageService = memberImageService;
        _memberService = memberService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ApproveMember(MemberChapterAdminServiceRequest request, Guid memberId)
    {
        var chapter = request.Chapter;

        var member = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId));

        var memberChapter = member.MemberChapter(chapter.Id);

        OdkAssertions.MemberOf(member, chapter.Id);
        OdkAssertions.Exists(memberChapter);

        memberChapter.Approved = true;

        _unitOfWork.MemberChapterRepository.Update(memberChapter);
        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendMemberApprovedEmail(request, chapter, member);

        return ServiceResult.Successful();
    }

    public async Task<AdminMemberAdminPageViewModel> GetAdminMemberViewModel(
        MemberChapterAdminServiceRequest request, Guid memberId)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var adminMembers = await _unitOfWork.ChapterAdminMemberRepository
            .GetByChapterId(chapter.Id).Run();

        var adminMember = adminMembers.FirstOrDefault(x => x.MemberId == memberId);
        OdkAssertions.Exists(adminMember);

        var currentAdminMember = adminMembers.FirstOrDefault(x => x.MemberId == currentMember.Id);

        AssertMemberIsChapterAdmin(request, currentAdminMember);

        // Owners can't have their role changed
        // Admins can edit other admins at or below their own level
        // Admins cannot change roles at their own level, except for their own
        var readOnly = !currentAdminMember.HasAccessTo(adminMember.Role, currentMember);
        var canEditRole = 
            !readOnly && 
            adminMember.Role != ChapterAdminRole.Owner && 
            (adminMember.MemberId == currentMember.Id || adminMember.Role != currentAdminMember?.Role);

        var roleOptions = new[]
        {
            ChapterAdminRole.Admin,
            ChapterAdminRole.Organiser
        }.Where(x => currentAdminMember.HasAccessTo(x, currentMember));

        return new AdminMemberAdminPageViewModel
        {
            AdminMember = adminMember,
            CanEditRole = canEditRole,
            ReadOnly = readOnly,
            RoleOptions = roleOptions.ToArray()
        };
    }

    public async Task<AdminMembersAdminPageViewModel> GetAdminMembersAdminPageViewModel(
        MemberChapterAdminServiceRequest request)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (adminMembers, members, ownerSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id));

        AssertMemberIsChapterAdmin(
            request,
            adminMembers.FirstOrDefault(x => x.MemberId == currentMember.Id));

        return new AdminMembersAdminPageViewModel
        {
            AdminMembers = adminMembers,
            Chapter = chapter,
            Members = members,
            OwnerSubscription = ownerSubscription,
            Platform = platform
        };
    }

    public async Task<BulkEmailAdminPageViewModel> GetBulkEmailViewModel(
        MemberChapterAdminServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        await AssertMemberIsChapterAdmin(request);

        return new BulkEmailAdminPageViewModel
        {
            Chapter = chapter,
            Platform = platform
        };
    }

    public async Task<Member> GetMember(MemberChapterAdminServiceRequest request, Guid memberId)
    {
        var member = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId));

        AssertMemberIsInChapter(member, request);

        return member;
    }

    public async Task<MemberApprovalsAdminPageViewModel> GetMemberApprovalsViewModel(
        MemberChapterAdminServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (members, membershipSettings) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetAllByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id));

        return new MemberApprovalsAdminPageViewModel
        {
            Chapter = chapter,
            MembershipSettings = membershipSettings,
            Pending = members
                .Where(x => x.MemberChapter(chapter.Id)?.Approved == false)
                .ToArray(),
            Platform = platform
        };
    }

    public async Task<MemberAvatar?> GetMemberAvatar(
        MemberChapterAdminServiceRequest request, Guid memberId)
    {
        var chapter = request.Chapter;

        var (member, memberAvatar) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberAvatarRepository.GetByMemberId(memberId));

        OdkAssertions.MemberOf(member, chapter.Id);

        return memberAvatar;
    }

    public async Task<MemberConversationsAdminPageViewModel> GetMemberConversationsViewModel(
        MemberChapterAdminServiceRequest request,
        Guid memberId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (member, conversations, ownerSubscription) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterConversationRepository.GetDtosByMemberId(memberId, chapter.Id),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id));

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

    public async Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberCsv(
        MemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var (members, subscriptions) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapter.Id));

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
                member.MemberChapter(chapter.Id)?.CreatedUtc.ToString("yyyy-MM-dd") ?? string.Empty,
                member.Activated ? "Y" : string.Empty,
                subscription?.ExpiresUtc?.ToString("yyyy-MM-dd") ?? string.Empty,
                subscription?.Type.ToString() ?? string.Empty
            ]);
        }

        return csv;
    }

    public async Task<MemberDeleteAdminPageViewModel> GetMemberDeleteViewModel(
        MemberChapterAdminServiceRequest request, Guid memberId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (member, subscription) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, chapter.Id));

        return new MemberDeleteAdminPageViewModel
        {
            Chapter = chapter,
            Member = member,
            Platform = platform,
            MemberSubscription = subscription
        };
    }

    public async Task<MemberEventsAdminPageViewModel> GetMemberEventsViewModel(
        MemberChapterAdminServiceRequest request, Guid memberId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (member, events, venues, memberResponses, invites) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.EventRepository.GetByChapterId(chapter.Id),
            x => x.VenueRepository.GetByChapterId(chapter.Id),
            x => x.EventResponseRepository.GetAllByMemberId(memberId, chapter.Id),
            x => x.EventInviteRepository.GetAllByMemberId(memberId, chapter.Id));

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

    public async Task<MemberImageAdminPageViewModel> GetMemberImageViewModel(
        MemberChapterAdminServiceRequest request, Guid memberId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (member, image, avatar) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberImageRepository.GetByMemberId(memberId),
            x => x.MemberAvatarRepository.GetByMemberId(memberId));

        OdkAssertions.MemberOf(member, chapter.Id);

        return new MemberImageAdminPageViewModel
        {
            Avatar = avatar,
            Chapter = chapter,
            Image = image,
            Member = member,
            Platform = platform
        };
    }

    public async Task<MemberPaymentsAdminPageViewModel> GetMemberPaymentsViewModel(
        MemberChapterAdminServiceRequest request, Guid memberId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (member, payments) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.PaymentRepository.GetMemberChapterPayments(memberId, chapter.Id));

        return new MemberPaymentsAdminPageViewModel
        {
            Chapter = chapter,
            Member = member,
            Payments = payments,
            Platform = platform,
        };
    }

    public async Task<SubscriptionCreateAdminPageViewModel> GetMemberSubscriptionCreateViewModel(
        MemberChapterAdminServiceRequest request)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (ownerSubscription,
            chapterPaymentSettings,
            chapterPaymentAccount,
            currency,
            sitePaymentSettings) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(chapter.Id),
            x => x.CurrencyRepository.GetByChapterId(chapter.Id),
            x => x.SitePaymentSettingsRepository.GetActive());

        return new SubscriptionCreateAdminPageViewModel
        {
            Chapter = chapter,
            Currency = currency,
            CurrentMember = currentMember,
            HasPaymentAccount = chapterPaymentAccount?.SetupComplete() == true,
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            SupportsRecurringPayments = sitePaymentSettings.SupportsRecurringPayments
        };
    }

    public async Task<SubscriptionsAdminPageViewModel> GetMemberSubscriptionsViewModel(
        MemberChapterAdminServiceRequest request)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (ownerSubscription,
            chapterSubscriptions,
            sitePaymentSettings,
            membershipSettings
        ) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterSubscriptionRepository.GetAdminDtosByChapterId(chapter.Id, includeDisabled: true),
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id));

        chapterSubscriptions = chapterSubscriptions
            .Where(x => x.ChapterSubscription.IsVisibleToAdmins(sitePaymentSettings))
            .ToArray();

        return new SubscriptionsAdminPageViewModel
        {
            Chapter = chapter,
            ChapterSubscriptions = chapterSubscriptions,
            MembershipSettings = membershipSettings ?? new(),
            OwnerSubscription = ownerSubscription,
            Platform = platform
        };
    }

    public async Task<SubscriptionAdminPageViewModel> GetMemberSubscriptionViewModel(
        MemberChapterAdminServiceRequest request, Guid subscriptionId)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (ownerSubscription,
            chapterPaymentAccount,
            subscription,
            defaultSitePaymentSettings) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(chapter.Id),
            x => x.ChapterSubscriptionRepository.GetById(subscriptionId),
            x => x.SitePaymentSettingsRepository.GetActive());

        OdkAssertions.BelongsToChapter(subscription, chapter.Id);

        var sitePaymentSettings = subscription.SitePaymentSettingId != null
            ? await _unitOfWork.SitePaymentSettingsRepository.GetById(subscription.SitePaymentSettingId.Value).Run()
            : defaultSitePaymentSettings;

        return new SubscriptionAdminPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            Currency = subscription.Currency,
            HasPaymentAccount = chapterPaymentAccount?.SetupComplete() == true,
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            Subscription = subscription,
            SupportsRecurringPayments = sitePaymentSettings.SupportsRecurringPayments
        };
    }

    public async Task<MembersAdminPageViewModel> GetMembersViewModel(MemberChapterAdminServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (membershipSettings, members, memberEmailPreferences, subscriptions) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetAllByChapterId(chapter.Id),
            x => x.MemberEmailPreferenceRepository.GetByChapterId(chapter.Id, MemberEmailPreferenceType.Events),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapter.Id));

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

    public async Task<MemberAdminPageViewModel> GetMemberViewModel(MemberChapterAdminServiceRequest request, Guid memberId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (member, subscription, notifications) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, chapter.Id),
            x => x.NotificationRepository.GetUnreadByMemberId(request.CurrentMember.Id, NotificationType.NewMember, memberId));

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

    public async Task<ServiceResult> RemoveMemberFromChapter(
        MemberChapterAdminServiceRequest request,
        Guid memberId,
        string? reason)
    {
        var chapter = request.Chapter;

        var (member, subscription) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, chapter.Id));

        if (subscription?.Type.IsPaid() == true && subscription?.IsExpired() == false)
        {
            return ServiceResult.Failure("You cannot remove members with an active paid subscription");
        }

        var deleteRequest = MemberChapterServiceRequest.Create(
            chapter, member, request);
        var result = await _memberService.DeleteMemberChapterData(deleteRequest);
        if (!result.Success)
        {
            return result;
        }

        await _memberEmailService.SendMemberDeleteEmail(request, chapter, member, reason);

        return ServiceResult.Successful();
    }

    public async Task RotateMemberImage(MemberChapterAdminServiceRequest request, Guid memberId)
    {
        var member = await GetMember(request, memberId);

        await _memberService.RotateMemberImage(member.Id);
    }

    public async Task SendActivationEmail(MemberChapterAdminServiceRequest request, Guid memberId)
    {
        var chapter = request.Chapter;

        var (member, memberActivationToken) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberActivationTokenRepository.GetByMemberId(memberId));

        AssertMemberIsInChapter(member, request);
        OdkAssertions.Exists(memberActivationToken);

        await _memberEmailService.SendActivationEmail(request, chapter, member, memberActivationToken.ActivationToken);
    }

    public async Task<ServiceResult> SendBulkEmail(
        MemberChapterAdminServiceRequest request, MemberFilter filter, string subject, string body)
    {
        var chapter = request.Chapter;

        var (members,
            memberEmailPreferences,
            memberSubscriptions,
            membershipSettings,
            subscription)
        = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberEmailPreferenceRepository.GetByChapterId(chapter.Id, MemberEmailPreferenceType.ChapterMessages),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id));

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

        await _memberEmailService.SendBulkEmail(request, chapter, filteredMembers, subject, body);

        return ServiceResult.Successful($"Bulk email sent to {filteredMembers.Length} members");
    }

    public async Task SendMemberSubscriptionReminderEmails(ServiceRequest request)
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
                if (memberChapter == null)
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
                    request,
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

    public async Task SetMemberVisibility(
        MemberChapterServiceRequest request, Guid memberId, bool visible)
    {
        var chapter = request.Chapter;

        var member = await GetSiteAdminRestrictedContent(request,
            x => x.MemberRepository.GetById(memberId));

        AssertMemberIsInChapter(member, request);

        var memberChapter = member.MemberChapter(chapter.Id) 
            ?? throw new OdkServiceException($"Member {memberId} not a member of chapter {chapter.Id}");

        memberChapter.HideProfile = !visible;

        _unitOfWork.MemberChapterRepository.Update(memberChapter);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> UpdateMemberImage(
        MemberChapterAdminServiceRequest request,
        Guid id,
        MemberImageUpdateModel model)
    {
        var chapter = request.Chapter;

        var (member, image, avatar) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(id),
            x => x.MemberImageRepository.GetByMemberId(id),
            x => x.MemberAvatarRepository.GetByMemberId(id));

        OdkAssertions.MemberOf(member, chapter.Id);

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

    public async Task<ServiceResult> UpdateMemberSubscription(
        MemberChapterAdminServiceRequest request,
        Guid memberId,
        MemberSubscriptionUpdateModel model)
    {
        var chapter = request.Chapter;

        var (member, memberSubscription) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, chapter.Id));

        var memberChapter = member.MemberChapter(chapter.Id);
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

    private static void AssertMemberIsInChapter(Member member, MemberChapterServiceRequest request)
        => OdkAssertions.MeetsCondition(member, x => x.IsMemberOf(request.Chapter.Id));

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
            memberSubscriptionsDictionary.TryGetValue(member.Id, out var memberSubscription);

            var subscriptionType = memberSubscription?.Type ?? SubscriptionType.Full;

            var status = _authorizationService.GetSubscriptionStatus(member, memberSubscription, membershipSettings);
            if (filter.Types.Contains(subscriptionType) &&
                filter.Statuses.Contains(status))
            {
                yield return member;
                continue;
            }
        }
    }

    private ServiceResult ValidateMemberSubscription(MemberSubscription subscription)
    {
        if (!Enum.IsDefined(subscription.Type) || subscription.Type == SubscriptionType.None)
        {
            return ServiceResult.Failure("Invalid type");
        }

        return ServiceResult.Successful();
    }
}
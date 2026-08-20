using System.Globalization;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
using ODK.Core.Workflows;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Emails;
using ODK.Services.Emails.Validation;
using ODK.Services.Events.ViewModels;
using ODK.Services.Exceptions;
using ODK.Services.Members.Models;
using ODK.Services.Members.ViewModels;
using ODK.Services.Members.Workflows.Account;
using ODK.Services.Members.Workflows.ChapterMembership;
using ODK.Services.Subscriptions;
using ODK.Services.Tasks;
using ODK.Services.Workflows;

namespace ODK.Services.Members;

public class MemberAdminService : OdkAdminServiceBase, IMemberAdminService
{
    private readonly StateMachineRunner<AccountState, AccountTrigger, AccountContext> _accountWorkflow;
    private readonly IAccountContextFactory _accountContextFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly IEmailValidationService _emailValidationService;
    private readonly StateMachineRunner<
        ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext> _chapterMembershipWorkflow;
    private readonly IChapterMembershipContextFactory _chapterMembershipContextFactory;
    private readonly IMemberChapterSubscriptionWriter _memberChapterSubscriptionWriter;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IMemberImageService _memberImageService;
    private readonly IMemberService _memberService;
    private readonly IServiceRequestFactory _serviceRequestFactory;
    private readonly IUnitOfWork _unitOfWork;

    public MemberAdminService(
        IUnitOfWork unitOfWork,
        IMemberService memberService,
        IAuthorizationService authorizationService,
        IMemberImageService memberImageService,
        IMemberEmailService memberEmailService,
        IBackgroundTaskService backgroundTaskService,
        IMemberChapterSubscriptionWriter memberChapterSubscriptionWriter,
        IEmailValidationService emailValidationService,
        StateMachineRunner<AccountState, AccountTrigger, AccountContext> account,
        IAccountContextFactory accountContextFactory,
        StateMachineRunner<ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext>
            chapterMembership,
        IChapterMembershipContextFactory chapterMembershipContextFactory,
        IServiceRequestFactory serviceRequestFactory)
        : base(unitOfWork)
    {
        _accountWorkflow = account;
        _accountContextFactory = accountContextFactory;
        _authorizationService = authorizationService;
        _chapterMembershipWorkflow = chapterMembership;
        _chapterMembershipContextFactory = chapterMembershipContextFactory;
        _backgroundTaskService = backgroundTaskService;
        _memberChapterSubscriptionWriter = memberChapterSubscriptionWriter;
        _memberEmailService = memberEmailService;
        _memberImageService = memberImageService;
        _memberService = memberService;
        _serviceRequestFactory = serviceRequestFactory;
        _emailValidationService = emailValidationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ApproveMember(IMemberChapterAdminServiceRequest request, Guid memberId)
    {
        var chapter = request.Chapter;

        /* Loaded here rather than by a context factory: the securable is enforced by the wrapper that does the
           loading, so a factory would have to sit inside it. The service loads, the factory maps. */
        var member = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId));

        OdkAssertions.MemberOf(member, chapter.Id);

        /* Approving a member who is already approved is a legal no-op rather than a failure, so there is no
           check for it here - the machine has an Approve edge out of Joined that does nothing. */
        var result = await _chapterMembershipWorkflow.Fire(
            ChapterMembershipTrigger.Approve,
            _chapterMembershipContextFactory.CreateForApproval(request, member));

        return result.ToServiceResult();
    }

    public async Task<AdminMemberAdminPageViewModel> GetAdminMemberViewModel(
        IMemberChapterAdminServiceRequest request, Guid memberId)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var adminMembers = await _unitOfWork.ChapterAdminMemberRepository
            .GetByChapterId(platform, chapter.Id).Run();

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
        IMemberChapterAdminServiceRequest request)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (adminMembers, members, ownerSubscriptionFeatures) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id),
            x => x.MemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRecordRepository
                .Query(x => x.Current().ForChapterOwner(chapter.Id).Active())
                .SiteSubscription()
                .Features()
                .GetAll());

        AssertMemberIsChapterAdmin(
            request,
            adminMembers.FirstOrDefault(x => x.MemberId == currentMember.Id));

        return new AdminMembersAdminPageViewModel
        {
            AdminMembers = adminMembers,
            Chapter = chapter,
            Members = members,
            OwnerSubscriptionFeatures = ownerSubscriptionFeatures
                .Select(x => x.Feature)
                .ToArray(),
            Platform = platform
        };
    }

    public async Task<BulkEmailAdminPageViewModel> GetBulkEmailViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var ownerSubscriptionFeatures = await GetChapterAdminRestrictedContent(request,
            x => x.MemberSiteSubscriptionRecordRepository
                .Query(x => x.Current().ForChapterOwner(chapter.Id).Active())
                .SiteSubscription()
                .Features()
                .GetAll());

        return new BulkEmailAdminPageViewModel
        {
            Chapter = chapter,
            OwnerSubscriptionFeatures = ownerSubscriptionFeatures
                .Select(x => x.Feature)
                .ToArray()
        };
    }

    public async Task<Member> GetMember(IMemberChapterAdminServiceRequest request, Guid memberId)
    {
        var member = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId));

        AssertMemberIsInChapter(member, request);

        return member;
    }

    public async Task<MemberApprovalsAdminPageViewModel> GetMemberApprovalsViewModel(
        IMemberChapterAdminServiceRequest request)
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

    public async Task<MemberConversationsAdminPageViewModel> GetMemberConversationsViewModel(
        IMemberChapterAdminServiceRequest request,
        Guid memberId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (member, conversations, ownerSubscriptionFeatures) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterConversationRepository.GetDtosByMemberId(memberId, chapter.Id),
            x => x.MemberSiteSubscriptionRecordRepository
                .Query(x => x.Current().ForChapterOwner(chapter.Id).Active())
                .SiteSubscription()
                .Features()
                .GetAll());

        OdkAssertions.MemberOf(member, chapter.Id);

        return new MemberConversationsAdminPageViewModel
        {
            Chapter = chapter,
            Conversations = conversations,
            Member = member,
            OwnerSubscriptionFeatures = ownerSubscriptionFeatures
                .Select(x => x.Feature)
                .ToArray(),
            Platform = platform
        };
    }

    public async Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberCsv(
        IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var (members, subscriptions) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberSubscriptionRecordRepository
                .Query()
                .Current()
                .ForChapter(chapter.Id)
                .ToChapterSubscription()
                .GetAll());

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
            .ToDictionary(x => x.MemberId);

        foreach (var member in members.OrderBy(x => x.FullName))
        {
            subscriptionDictionary.TryGetValue(member.Id, out var subscription);

            csv.Add(
            [
                member.Id.ToString(),
                member.FirstName,
                member.LastName,
                member.MemberChapter(chapter.Id)?.CreatedUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                member.Activated ? "Y" : string.Empty,
                subscription?.ExpiresUtc?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                subscription?.Type.ToString() ?? string.Empty
            ]);
        }

        return csv;
    }

    public async Task<MemberDeleteAdminPageViewModel> GetMemberDeleteViewModel(
        IMemberChapterAdminServiceRequest request, Guid memberId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (member, subscription) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRecordRepository
                .Query()
                .Current()
                .ForMember(memberId)
                .ForChapter(chapter.Id)
                .ToChapterSubscription()
                .GetSingleOrDefault());

        return new MemberDeleteAdminPageViewModel
        {
            Chapter = chapter,
            Member = member,
            Platform = platform,
            MemberSubscription = subscription
        };
    }

    public async Task<MemberEventsAdminPageViewModel> GetMemberEventsViewModel(
        IMemberChapterAdminServiceRequest request, Guid memberId)
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
        IMemberChapterAdminServiceRequest request, Guid memberId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var memberDto = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetWithAvatarById(memberId));

        OdkAssertions.MemberOf(memberDto.Member, chapter.Id);

        return new MemberImageAdminPageViewModel
        {
            AvatarVersion = memberDto.AvatarVersion,
            Chapter = chapter,
            Member = memberDto.Member,
            Platform = platform
        };
    }

    public async Task<MemberPaymentsAdminPageViewModel> GetMemberPaymentsViewModel(
        IMemberChapterAdminServiceRequest request, Guid memberId)
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
        IMemberChapterAdminServiceRequest request)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (ownerSubscriptionFeatures,
            chapterPaymentSettings,
            chapterPaymentAccount,
            currency,
            sitePaymentSettings) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberSiteSubscriptionRecordRepository
                .Query(x => x.Current().ForChapterOwner(chapter.Id).Active())
                .SiteSubscription()
                .Features()
                .GetAll(),
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
            OwnerSubscriptionFeatures = ownerSubscriptionFeatures
                .Select(x => x.Feature)
                .ToArray(),
            Platform = platform,
            SupportsRecurringPayments = sitePaymentSettings.SupportsRecurringPayments
        };
    }

    public async Task<SubscriptionsAdminPageViewModel> GetMemberSubscriptionsViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (ownerSubscriptionFeatures,
            chapterSubscriptions,
            sitePaymentSettings,
            membershipSettings
        ) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberSiteSubscriptionRecordRepository
                .Query(x => x.Current().ForChapterOwner(chapter.Id).Active())
                .SiteSubscription()
                .Features()
                .GetAll(),
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
            OwnerSubscriptionFeatures = ownerSubscriptionFeatures
                .Select(x => x.Feature)
                .ToArray(),
            Platform = platform
        };
    }

    public async Task<SubscriptionAdminPageViewModel> GetMemberSubscriptionViewModel(
        IMemberChapterAdminServiceRequest request, Guid subscriptionId)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (ownerSubscriptionFeatures,
            chapterPaymentAccount,
            subscription,
            defaultSitePaymentSettings) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberSiteSubscriptionRecordRepository
                .Query(x => x.Current().ForChapterOwner(chapter.Id).Active())
                .SiteSubscription()
                .Features()
                .GetAll(),
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
            OwnerSubscriptionFeatures = ownerSubscriptionFeatures
                .Select(x => x.Feature)
                .ToArray(),
            Platform = platform,
            Subscription = subscription,
            SupportsRecurringPayments = sitePaymentSettings.SupportsRecurringPayments
        };
    }

    public async Task<MembersAdminPageViewModel> GetMembersViewModel(IMemberChapterAdminServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (membershipSettings, members, memberEmailPreferences, subscriptions) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetAllWithAvatarByChapterId(chapter.Id),
            x => x.MemberEmailPreferenceRepository.GetByChapterId(chapter.Id, MemberEmailPreferenceType.Events),
            x => x.MemberSubscriptionRecordRepository
                .Query()
                .Current()
                .ForChapter(chapter.Id)
                .ToChapterSubscription()
                .GetAll());

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

    public async Task<MemberAdminPageViewModel> GetMemberViewModel(IMemberChapterAdminServiceRequest request, Guid memberId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (member, subscription, notifications) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRecordRepository
                .Query()
                .Current()
                .ForMember(memberId)
                .ForChapter(chapter.Id)
                .ToChapterSubscription()
                .GetSingleOrDefault(),
            x => x.NotificationRepository.GetUnreadByMemberId(request.CurrentMember.Id, NotificationType.NewMember, memberId));

        OdkAssertions.MemberOf(member, chapter.Id);

        if (notifications.Count > 0)
        {
            _unitOfWork.NotificationRepository.MarkAsRead(notifications);
            await _unitOfWork.SaveChanges();
        }

        return new MemberAdminPageViewModel
        {
            Chapter = chapter,
            Member = member,
            Platform = platform,
            Subscription = subscription
        };
    }

    public async Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberImportTemplate(
        IMemberChapterAdminServiceRequest request)
    {
        await AssertMemberIsChapterAdmin(request);

        return [MemberImportModel.GetCsvHeaderRow()];
    }

    public async Task<MemberImportPreview> GetMemberImportPreview(
        IMemberChapterAdminServiceRequest request, IReadOnlyCollection<MemberImportModel> members)
    {
        var chapter = request.Chapter;

        var emailAddresses = members
            .Select(x => x.EmailAddress)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var existingMembers = await GetChapterAdminRestrictedContent(request,
            x => x.MemberRepository
                .Query()
                .HasEmailAddress(emailAddresses)
                .GetAll());

        var existingMemberDictionary = existingMembers
            .ToDictionary(x => x.EmailAddress, StringComparer.OrdinalIgnoreCase);

        var distinctMembers = DistinctByEmailAddress(members);

        // Soft only: a file can hold hundreds of rows, and a deliverability check per row would spend
        // the daily quota on a single import. Format alone still catches the typos that matter.
        var validity = await ValidateImportEmailAddresses(distinctMembers);

        var rows = distinctMembers
            .Select(x =>
            {
                existingMemberDictionary.TryGetValue(x.EmailAddress, out var member);

                var status = !validity[x.EmailAddress]
                    ? MemberImportRowStatus.Invalid
                    : member == null
                        ? MemberImportRowStatus.New
                        : member.IsMemberOf(chapter.Id)
                            ? MemberImportRowStatus.ExistingInGroup
                            : MemberImportRowStatus.ExistingNotInGroup;

                return new MemberImportPreviewRow
                {
                    Member = x,
                    Status = status
                };
            })
            .ToList();

        return new MemberImportPreview
        {
            Rows = rows
        };
    }

    public async Task<SiteAdminFlaggedMembersViewModel> GetSiteAdminFlaggedMembersViewModel(
        IMemberServiceRequest request)
    {
        var members = await GetSiteAdminRestrictedContent(request,
            x => x.MemberRepository
                .Query(x => x.Flagged())
                .GetAll());

        return new SiteAdminFlaggedMembersViewModel
        {
            Rows = members
                .OrderByDescending(x => x.CreatedUtc)
                .Select(x => new SiteAdminFlaggedMembersRowViewModel
                {
                    Activated = x.Activated,
                    CreatedUtc = x.CreatedUtc,
                    EmailAddress = x.EmailAddress,
                    FullName = x.FullName,
                    MemberId = x.Id,
                    RecaptchaScore = x.RecaptchaScore ?? 0
                })
                .ToArray()
        };
    }

    public async Task<ServiceResult> ImportMembers(IMemberChapterAdminServiceRequest request, IReadOnlyCollection<MemberImportModel> members)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var emailAddresses = members
            .Select(x => x.EmailAddress)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var (siteSubscription, chapterLocation, currency, country, existingMembers, outstandingInvites) = await GetChapterAdminRestrictedContent(request,
            x => x.SiteSubscriptionRepository.GetDefault(platform),
            x => x.ChapterLocationRepository.GetByChapterId(chapter.Id),
            x => x.CurrencyRepository.GetByChapterId(chapter.Id),
            x => x.CountryRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository
                .Query()
                .HasEmailAddress(emailAddresses)
                .GetAll(),
            x => x.MemberChapterInviteRepository.GetByChapterId(chapter.Id));

        // Read once for the whole file: every row's context is a projection of this, so a file of a thousand
        // rows costs the same queries as a file of one.
        var batch = new ImportBatch
        {
            ChapterLocation = chapterLocation,
            Country = country,
            Currency = currency,
            ExistingMembers = existingMembers,
            OutstandingInvites = outstandingInvites,
            SiteSubscription = siteSubscription
        };

        // De-duplicate the incoming rows by email (case-insensitively) so a file that contains the
        // same address more than once does not create multiple members for it.
        var distinctMembers = DistinctByEmailAddress(members);

        // The same check the preview showed, so what gets imported matches what was displayed. Batched, because
        // it calls out to a verifier.
        var validity = await ValidateImportEmailAddresses(distinctMembers);

        var activationEmailMembers = new List<Member>();
        var inviteEmailMembers = new List<Member>();

        foreach (var importMember in distinctMembers)
        {
            if (!validity[importMember.EmailAddress])
            {
                continue;
            }

            /* Raising the account and asking the member to join are two machines, and both only stage writes -
               the file is committed once, below. Neither trigger being permitted is how a row that needs nothing
               doing is skipped: an address that already has an account cannot be imported again, and a member
               already invited or already in the group cannot be invited again. */
            var accountContext = _accountContextFactory.CreateForImport(request, importMember, batch);
            var raised = await _accountWorkflow.Fire(AccountTrigger.Import, accountContext);

            var member = accountContext.NewMember ?? accountContext.Member;
            if (member == null)
            {
                continue;
            }

            var invited = await _chapterMembershipWorkflow.Fire(
                ChapterMembershipTrigger.Invite,
                _chapterMembershipContextFactory.CreateForInvite(
                    request, member, batch.OutstandingInvite(member.Id)));

            if (!invited.Success)
            {
                continue;
            }

            /* Which email a new member gets is decided by whether they can act on the invitation's link.
               Signing up on Drunken Knitwits is joining the group, so the link lands on the join page with
               their details already filled in and the account is created from what they submit - while an
               activation link would take them straight past that page into an account belonging to no group.
               Group Squirrel's join page requires an account, so a new member there has to activate first. A
               member who already had an account can act on it whichever platform they are on. */
            var canActOnTheInvitation = !raised.Success || platform == PlatformType.DrunkenKnitwits;

            (canActOnTheInvitation ? inviteEmailMembers : activationEmailMembers).Add(member);
        }

        await _unitOfWork.SaveChanges();

        // Send the activation/invite emails in the background so a large import doesn't block the request,
        // and so each email is an independently-retryable job. Only ids cross the Hangfire boundary; the
        // member, chapter and token are reloaded inside each job.
        var jobRequest = JobRequest.Create(request);

        foreach (var member in activationEmailMembers)
        {
            EnqueueSendImportActivationEmailJob(jobRequest, chapter.Id, member.Id);
        }

        foreach (var member in inviteEmailMembers)
        {
            EnqueueSendImportInviteEmailJob(jobRequest, chapter.Id, member.Id);
        }

        return ServiceResult.Successful();
    }

    /* Public for Hangfire, which needs a method to bind to, and called by nothing else: each turns the
       job's ids back into a request and hands off to the work. These signatures are a wire format - see
       JobRequest - so a change to one is a change every queued job of that kind has to survive. */
    public async Task SendImportActivationEmailJob(JobRequest request, Guid chapterId, Guid memberId)
        => await SendImportActivationEmail(await _serviceRequestFactory.Create(request), chapterId, memberId);

    /// <inheritdoc cref="SendImportActivationEmailJob" />
    public async Task SendImportInviteEmailJob(JobRequest request, Guid chapterId, Guid memberId)
        => await SendImportInviteEmail(await _serviceRequestFactory.Create(request), chapterId, memberId);

    public async Task SendImportActivationEmail(IServiceRequest request, Guid chapterId, Guid memberId)
    {
        var (member, chapter, activationToken) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterRepository.GetById(request.Platform, chapterId),
            x => x.MemberActivationTokenRepository.GetByMemberId(memberId));

        // The token is removed once the member activates; there is nothing to send if that has happened.
        if (activationToken == null)
        {
            return;
        }

        var emailRequest = MemberChapterServiceRequest.Create(chapter, member, request);
        await _memberEmailService.SendMemberImportActivationEmail(
            emailRequest, activationToken.ActivationToken);
    }

    public async Task SendImportInviteEmail(IServiceRequest request, Guid chapterId, Guid memberId)
    {
        var (member, chapter, invite) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterRepository.GetById(request.Platform, chapterId),
            x => x.MemberChapterInviteRepository.GetByMemberId(memberId, chapterId));

        // Consumed once they join, and the link is worthless without it, so there is nothing to send.
        if (invite == null)
        {
            return;
        }

        var chapterRequest = ChapterServiceRequest.Create(chapter, request);
        await _memberEmailService.SendMemberImportInviteEmail(chapterRequest, member, invite.Token);
    }

    public async Task<ServiceResult> RemoveMemberFromChapter(
        IMemberChapterAdminServiceRequest request,
        Guid memberId,
        string? reason)
    {
        var chapter = request.Chapter;

        var (member, subscription) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRecordRepository
                .Query()
                .Current()
                .ForMember(memberId)
                .ForChapter(chapter.Id)
                .ToChapterSubscription()
                .GetSingleOrDefault());

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

        await _memberEmailService.SendMemberDeleteEmail(
            request,
            member,
            reason);

        return ServiceResult.Successful();
    }

    public async Task RotateMemberImage(IMemberChapterAdminServiceRequest request, Guid memberId)
    {
        var member = await GetMember(request, memberId);

        await _memberService.RotateMemberImage(member.Id);
    }

    public async Task SendActivationEmail(IMemberChapterAdminServiceRequest request, Guid memberId)
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
        IMemberChapterAdminServiceRequest request, MemberFilter filter, string subject, string body)
    {
        var chapter = request.Chapter;

        var (members,
            memberEmailPreferences,
            memberSubscriptions,
            membershipSettings,
            hasAccess)
        = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberEmailPreferenceRepository.GetByChapterId(chapter.Id, MemberEmailPreferenceType.ChapterMessages),
            x => x.MemberSubscriptionRecordRepository
                .Query()
                .Current()
                .ForChapter(chapter.Id)
                .ToChapterSubscription()
                .GetAll(),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRecordRepository
                .Query(x => x.Current().ForChapterOwner(chapter.Id).Active())
                .HasFeature(SiteFeatureType.SendMemberEmails));

        if (!hasAccess)
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

        await _memberEmailService.SendBulkEmail(
            request,
            filteredMembers,
            subject,
            body);

        return ServiceResult.Successful($"Bulk email sent to {filteredMembers.Length} members");
    }

    public async Task SendMemberSubscriptionReminderEmails(IServiceRequest request)
    {
        var platform = request.Platform;

        var chapters = await _unitOfWork.ChapterRepository
            .GetAll(platform, includeUnpublished: false)
            .Run();

        var chapterIds = chapters.Select(x => x.Id).ToArray();

        // Load members, subscriptions and settings for every chapter in a single round-trip rather than
        // querying per chapter (which was N round-trips - an N+1).
        var (members, memberSubscriptions, allMembershipSettings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetByChapterIds(chapterIds),
            x => x.MemberSubscriptionRecordRepository
                .Query()
                .Current()
                .InChapters(chapterIds)
                .ToChapterSubscription()
                .GetAll(),
            x => x.ChapterMembershipSettingsRepository.GetByChapterIds(chapterIds));

        var membershipSettingsByChapterId = allMembershipSettings.ToDictionary(x => x.ChapterId);

        var memberSubscriptionsByMemberChapter = memberSubscriptions
            .ToDictionary(x => (x.ChapterId, x.MemberId));

        // Group members by each (non-hidden) chapter they belong to - mirrors the per-chapter
        // GetByChapterId filter that was previously applied in the loop.
        var chapterIdSet = chapterIds.ToHashSet();
        var membersByChapterId = members
            .SelectMany(member => member.Chapters
                .Where(memberChapter =>
                    !memberChapter.HideProfile && chapterIdSet.Contains(memberChapter.ChapterId))
                .Select(memberChapter => (memberChapter.ChapterId, Member: member)))
            .GroupBy(x => x.ChapterId)
            .ToDictionary(x => x.Key, x => x.Select(m => m.Member).ToArray());

        foreach (var chapter in chapters)
        {
            if (!membershipSettingsByChapterId.TryGetValue(chapter.Id, out var membershipSettings) ||
                !membershipSettings.Enabled)
            {
                continue;
            }

            if (!membersByChapterId.TryGetValue(chapter.Id, out var chapterMembers))
            {
                continue;
            }

            foreach (var member in chapterMembers)
            {
                var memberChapter = member.MemberChapter(chapter.Id);
                if (memberChapter == null)
                {
                    continue;
                }

                if (!memberSubscriptionsByMemberChapter.TryGetValue((chapter.Id, member.Id), out var memberSubscription))
                {
                    _memberChapterSubscriptionWriter.MakeRecordCurrent(
                        newRecord: new MemberSubscriptionRecord
                        {
                            ChapterId = chapter.Id,
                            MemberId = member.Id,
                            PurchasedUtc = DateTime.UtcNow,
                            Type = SubscriptionType.Trial
                        },
                        existingCurrent: null);
                    continue;
                }

                if (memberSubscription.ExpiresUtc == null)
                {
                    continue;
                }

                // Send within a window around expiry - from 7 days before to 7 days after. There is no
                // sent-state tracking, so a member may receive a reminder on each scheduled run in the window.
                var now = DateTime.UtcNow;
                var expires = memberSubscription.ExpiresUtc.Value;
                if (expires > now.AddDays(7) || expires < now.AddDays(-7))
                {
                    continue;
                }

                var disabledDate = expires
                    .AddDays(membershipSettings.MembershipDisabledAfterDaysExpired);

                await _memberEmailService.SendMemberChapterSubscriptionExpiringEmail(
                    ChapterServiceRequest.Create(chapter, request),
                    member,
                    memberSubscription,
                    expires: expires,
                    disabledDate: disabledDate);
            }
        }

        // Persist any trial records created above for members that had none.
        await _unitOfWork.SaveChanges();
    }

    public async Task SetMemberVisibility(
        IMemberChapterServiceRequest request, Guid memberId, bool visible)
    {
        var chapter = request.Chapter;

        var member = await GetSiteAdminRestrictedContent(request,
            x => x.MemberRepository.GetById(memberId));

        AssertMemberIsInChapter(member, request);

        var memberChapter = member.MemberChapter(chapter.Id)
            ?? throw new OdkServiceException($"Member {memberId} not a member of chapter {chapter.Id}");

        memberChapter.HideProfile = !visible;

        _unitOfWork.MemberChapterRepository.Update(memberChapter);

        await _unitOfWork.SaveChanges();
    }

    public async Task<ServiceResult> UpdateMemberImage(
        IMemberChapterAdminServiceRequest request,
        Guid id,
        MemberImageUpdateModel model)
    {
        var chapter = request.Chapter;

        var (member, avatar) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(id),
            x => x.MemberAvatarRepository.GetByMemberId(id));

        OdkAssertions.MemberOf(member, chapter.Id);

        avatar ??= new MemberAvatar();

        var result = _memberImageService.UpdateMemberImage(avatar, model.ImageData);
        if (!result.Success)
        {
            return result;
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

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful("Picture updated");
    }

    public async Task<ServiceResult> UpdateMemberSubscription(
        IMemberChapterAdminServiceRequest request,
        Guid memberId,
        MemberSubscriptionUpdateModel model)
    {
        var chapter = request.Chapter;

        if (!Enum.IsDefined(model.Type) || model.Type == SubscriptionType.None)
        {
            return ServiceResult.Failure("Invalid type");
        }

        var (member, currentRecord) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSubscriptionRecordRepository
                .Query()
                .Current()
                .ForMember(memberId)
                .ForChapter(chapter.Id)
                .GetSingleOrDefault());

        if (member.MemberChapter(chapter.Id) == null)
        {
            return ServiceResult.Failure("Member chapter not found");
        }

        _memberChapterSubscriptionWriter.MakeRecordCurrent(
            newRecord: new MemberSubscriptionRecord
            {
                ChapterId = chapter.Id,
                ExpiresUtc = model.ExpiryDate,
                MemberId = memberId,
                PurchasedUtc = DateTime.UtcNow,
                Type = model.Type
            },
            existingCurrent: currentRecord);

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    private static void AssertMemberIsInChapter(Member member, IMemberChapterServiceRequest request)
        => OdkAssertions.MeetsCondition(member, x => x.IsMemberOf(request.Chapter.Id));

    // Collapses import rows that share an email address (case-insensitively) to a single row, keeping
    // the first occurrence. Used by both the preview and the commit so they agree on the row count.
    private static IReadOnlyCollection<MemberImportModel> DistinctByEmailAddress(
        IReadOnlyCollection<MemberImportModel> members)
        => members
            .GroupBy(x => x.EmailAddress, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToArray();

    private string EnqueueSendImportActivationEmailJob(JobRequest request, Guid chapterId, Guid memberId)
        => _backgroundTaskService.Enqueue(
            () => SendImportActivationEmailJob(request, chapterId, memberId),
            BackgroundTaskQueueType.Emails);

    private string EnqueueSendImportInviteEmailJob(JobRequest request, Guid chapterId, Guid memberId)
        => _backgroundTaskService.Enqueue(
            () => SendImportInviteEmailJob(request, chapterId, memberId),
            BackgroundTaskQueueType.Emails);

    private IEnumerable<Member> FilterMembers(
        IEnumerable<Member> members,
        IEnumerable<MemberChapterSubscription> memberSubscriptions,
        ChapterMembershipSettings? membershipSettings,
        MemberFilter filter)
    {
        var memberSubscriptionsDictionary = memberSubscriptions
            .ToDictionary(x => x.MemberId);

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

    /// <summary>
    /// Whether each row's address is a usable format, keyed by the address. Soft only - see the callers.
    /// </summary>
    private async Task<IReadOnlyDictionary<string, bool>> ValidateImportEmailAddresses(
        IReadOnlyCollection<MemberImportModel> members)
    {
        var validity = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        foreach (var member in members)
        {
            var result = await _emailValidationService.Validate(
                member.EmailAddress, EmailValidationLevel.Soft);
            validity[member.EmailAddress] = result.Success;
        }

        return validity;
    }
}

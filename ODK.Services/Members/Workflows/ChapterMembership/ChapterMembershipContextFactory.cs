using ODK.Core.Chapters;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Subscriptions;
using ODK.Core.Notifications;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Members.Models;
using ODK.Services.Members.Workflows.Account;

namespace ODK.Services.Members.Workflows.ChapterMembership;

public sealed class ChapterMembershipContextFactory : IChapterMembershipContextFactory
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUnitOfWork _unitOfWork;

    public ChapterMembershipContextFactory(IUnitOfWork unitOfWork, IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// An admin approving a queued member. Synchronous and issues no query: the securable is enforced by the
    /// wrapper that loads the member, so the service loads and this maps.
    /// </summary>
    public ChapterMembershipContext CreateForApproval(IChapterServiceRequest request, Member member) => new()
    {
        /* Approving writes the membership row and emails the member, so everything the join transitions read
           is empty here - the invitation included. The member is already in the group, so the row rather than
           an invitation is what the state is derived from. */
        AdminMembers = [],
        ApprovalRequired = false,
        ChapterId = request.Chapter.Id,
        ChapterProperties = [],
        Member = member,
        MemberCount = 0,
        MemberProperties = [],
        NotificationSettings = [],
        OwnerSubscriptionFeatures = [],
        Platform = request.Platform,
        Properties = [],
        Request = request
    };

    /// <summary>
    /// One row of an import. Synchronous and issues no query for the same reason as its account counterpart:
    /// the whole file's data is read once.
    /// </summary>
    public ChapterMembershipContext CreateForInvite(
        IChapterServiceRequest request,
        Member member,
        MemberChapterInvite? outstandingInvite) => new()
    {
        /* An invitation notifies nobody, queues nobody and asks nothing of the member, so everything the join
           transitions read is empty here. What the machine needs is the member, the group, and whether an
           invitation is already outstanding - which together decide whether Invite is permitted at all. */
        AdminMembers = [],
        ApprovalRequired = false,
        ChapterId = request.Chapter.Id,
        ChapterProperties = [],
        Invite = outstandingInvite,
        Member = member,
        MemberCount = 0,
        MemberProperties = [],
        NotificationSettings = [],
        OwnerSubscriptionFeatures = [],
        Platform = request.Platform,
        Properties = [],
        Request = request
    };

    public ChapterMembershipContext CreateForGroupSignUp(AccountContext context)
    {
        var chapter = context.RequiredChapter;
        var member = context.RequiredNewMember;
        var properties = context.RequiredProfile.Properties.ToArray();

        return new ChapterMembershipContext
        {
            /* No admin members and no notification settings: a group sign-up notifies nobody. The group hears
               about a new member when the account is activated, not when it is created. */
            AdminMembers = [],
            ApprovalRequired = ApprovalIsRequired(context.OwnerSubscriptionFeatures, context.MembershipSettings),
            ChapterId = chapter.Id,
            ChapterProperties = context.ChapterProperties,
            Invite = context.Invite,
            Member = member,
            MemberCount = context.MemberCount,
            MemberProperties = properties.Select(x => x.ToMemberProperty(member.Id)).ToArray(),
            MembershipSettings = context.MembershipSettings,
            NotificationSettings = [],
            OwnerSubscription = context.OwnerSubscription,
            OwnerSubscriptionFeatures = context.OwnerSubscriptionFeatures,
            Platform = context.Request.Platform,
            Properties = properties,
            Request = (IChapterServiceRequest)context.Request
        };
    }

    public async Task<ChapterMembershipContext> CreateForJoin(
        IMemberChapterServiceRequest request,
        IEnumerable<MemberPropertyUpdateModel> properties)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (
            adminMembers,
            notificationSettings,
            ownerSubscription,
            memberCount,
            chapterProperties,
            membershipSettings,
            invite
        ) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(chapter.Id, NotificationType.NewMember),
            x => x.MemberSiteSubscriptionRecordRepository
                .Query(x => x.Current().ForChapterOwner(chapter.Id).Active())
                .SiteSubscription()
                .WithFeatures()
                .GetSingleOrDefault(),
            x => x.MemberRepository.GetCountByChapterId(chapter.Id),
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.MemberChapterInviteRepository.GetByMemberId(currentMember.Id, chapter.Id));

        var features = ownerSubscription?.Features ?? [];
        var propertyUpdates = properties.ToArray();

        return new ChapterMembershipContext
        {
            AdminMembers = adminMembers,
            /* The group's setting only. Whether this member is queued also turns on their invitation, which the
               machine carries as a state - see ChapterMembershipContext.ApprovedOnJoining. */
            ApprovalRequired = ApprovalIsRequired(features, membershipSettings),
            ChapterId = chapter.Id,
            ChapterProperties = chapterProperties,
            Invite = invite,
            Member = currentMember,
            MemberCount = memberCount,
            MemberProperties = propertyUpdates
                .Select(x => x.ToMemberProperty(currentMember.Id))
                .ToArray(),
            MembershipSettings = membershipSettings,
            NotificationSettings = notificationSettings,
            OwnerSubscription = ownerSubscription?.SiteSubscription,
            OwnerSubscriptionFeatures = features,
            Platform = platform,
            Properties = propertyUpdates,
            Request = request
        };
    }

    /// <summary>
    /// The group's setting, and whether its owner's subscription carries the feature at all. Whether *this*
    /// member is queued also turns on their invitation, which the machine carries as a state - see
    /// <see cref="ChapterMembershipContext.ApprovedOnJoining"/>.
    /// </summary>
    private bool ApprovalIsRequired(
        IReadOnlyCollection<SiteSubscriptionFeature> features,
        ChapterMembershipSettings? membershipSettings)
        => _authorizationService.ChapterHasAccess(features, SiteFeatureType.ApproveMembers) &&
           membershipSettings?.ApproveNewMembers == true;
}

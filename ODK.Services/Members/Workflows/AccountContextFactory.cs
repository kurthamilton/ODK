using ODK.Core.Features;
using ODK.Core.Notifications;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Members.Models;

namespace ODK.Services.Members.Workflows;

public sealed class AccountContextFactory : IAccountContextFactory
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUnitOfWork _unitOfWork;

    public AccountContextFactory(IUnitOfWork unitOfWork, IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountContext> CreateForJoin(
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

        return new AccountContext
        {
            AdminMembers = adminMembers,
            /* An invitation is approval: the group asked this member in, so putting them in the queue would be
               asking it to approve someone it invited. Decided here rather than in a guard because the feature
               check and the group's settings are both loaded state. */
            ApprovalRequired =
                invite == null &&
                _authorizationService.ChapterHasAccess(features, SiteFeatureType.ApproveMembers) &&
                membershipSettings?.ApproveNewMembers == true,
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
            Request = request,
            VerifiedByOAuth = false
        };
    }
}

using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Core.Notifications;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Core.Venues;
using ODK.Services.Notifications.ViewModels;

namespace ODK.Services.Notifications;

public interface INotificationService
{
    void AddEventWaitlistPromotionNotifications(
        Event @event,
        IEnumerable<Member> members,
        IEnumerable<MemberNotificationSettings> settings);

    void AddNewChapterContactMessageNotifications(
        ChapterContactMessage message,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        IReadOnlyCollection<MemberNotificationSettings> settings);

    void AddNewConversationAdminMessageNotifications(
        ChapterConversation conversation,
        Member member,
        IReadOnlyCollection<MemberNotificationSettings> settings);

    void AddNewConversationOwnerMessageNotifications(
        ChapterConversation conversation,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        IReadOnlyCollection<MemberNotificationSettings> settings);

    Task AddNewEventNotifications(
        Event @event,
        Venue venue,
        IReadOnlyCollection<Member> members,
        IReadOnlyCollection<MemberNotificationSettings> settings);

    void AddNewMemberNotifications(
        Member member,
        Guid chapterId,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        IReadOnlyCollection<MemberNotificationSettings> settings);

    void AddSiteConversationMemberMessageNotifications(
        SiteConversation conversation,
        IReadOnlyCollection<Member> siteAdmins,
        IReadOnlyCollection<MemberNotificationSettings> settings);

    void AddSiteConversationReplyNotification(
        SiteConversation conversation,
        Member member,
        IReadOnlyCollection<MemberNotificationSettings> settings);

    /// <summary>
    /// Tells members their lapsed site subscription has been moved onto <paramref name="siteSubscription"/>,
    /// the platform's free plan. Raised by the sweep that moves them, so nothing they did prompts it.
    /// </summary>
    void AddSubscriptionDowngradedNotifications(
        SiteSubscription siteSubscription,
        IEnumerable<Member> members,
        IEnumerable<MemberNotificationSettings> settings);

    Task AddSubscriptionRenewedNotification(
        Member member,
        Chapter? chapter,
        Payment payment,
        DateTime? nextPaymentUtc,
        IReadOnlyCollection<MemberNotificationSettings> settings);

    Task<NotificationsPageViewModel> GetNotificationsPageViewModel(IMemberServiceRequest request);

    Task<UnreadNotificationsViewModel> GetUnreadNotificationsViewModel(IMemberServiceRequest request);

    Task MarkAllAsRead(Guid memberId);

    Task MarkAsRead(Guid memberId, Guid notificationId);

    Task<ServiceResult> UpdateMemberNotificationSettings(
        IMemberServiceRequest request,
        NotificationGroupType group,
        bool enabled);

    Task<ServiceResult> UpdateMemberChapterNotificationSettings(
        IMemberChapterServiceRequest request,
        NotificationGroupType group,
        bool enabled);
}
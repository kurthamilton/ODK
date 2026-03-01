using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Notifications;
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

    void AddNewEventNotifications(
        Chapter chapter,
        Event @event,
        Venue venue,
        IReadOnlyCollection<Member> members,
        IReadOnlyCollection<MemberNotificationSettings> settings);

    void AddNewMemberNotifications(
        Member member,
        Guid chapterId,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
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
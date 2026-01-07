using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Venues;
using ODK.Services.Notifications.ViewModels;

namespace ODK.Services.Notifications;

public interface INotificationService
{
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

    Task<NotificationsPageViewModel> GetNotificationsPageViewModel(Guid memberId);

    Task<UnreadNotificationsViewModel> GetUnreadNotificationsViewModel(MemberServiceRequest request);

    Task MarkAsRead(Guid memberId, Guid notificationId);

    Task<ServiceResult> UpdateMemberNotificationSettings(
        Guid memberId,
        IReadOnlyCollection<NotificationType> disabledTypes);
}

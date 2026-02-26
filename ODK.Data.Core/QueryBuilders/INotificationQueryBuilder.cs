using ODK.Core.Notifications;
using ODK.Data.Core.Notifications;

namespace ODK.Data.Core.QueryBuilders;

public interface INotificationQueryBuilder : IDatabaseEntityQueryBuilder<Notification, INotificationQueryBuilder>
{
    INotificationQueryBuilder ForChapter(Guid chapterId);

    INotificationQueryBuilder ForEntity(Guid entityId);

    INotificationQueryBuilder ForMember(Guid memberId);

    INotificationQueryBuilder ForType(NotificationType type);

    IQueryBuilder<NotificationDto> ToDto();

    INotificationQueryBuilder Unread();
}
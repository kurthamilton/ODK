using ODK.Core.Notifications;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Notifications;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface INotificationRepository : IReadWriteRepository<Notification, INotificationQueryBuilder>
{
    IDeferredQueryMultiple<Notification> GetByMemberId(Guid memberId, Guid chapterId);

    IDeferredQueryMultiple<Notification> GetUnreadByEntityId(NotificationType type, Guid entityId);

    IDeferredQueryMultiple<Notification> GetUnreadByMemberId(Guid memberId);

    IDeferredQueryMultiple<NotificationDto> GetUnreadDtosByMemberId(Guid memberId);

    IDeferredQueryMultiple<Notification> GetUnreadByMemberId(Guid memberId, NotificationType type, Guid entityId);

    void MarkAsRead(IEnumerable<Notification> notifications);
}
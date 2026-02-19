using ODK.Core.Notifications;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface INotificationRepository : IReadWriteRepository<Notification>
{
    IDeferredQueryMultiple<Notification> GetByMemberId(Guid memberId, Guid chapterId);

    IDeferredQuery<int> GetCountByMemberId(Guid memberId);

    IDeferredQueryMultiple<Notification> GetUnreadByChapterId(Guid chapterId, NotificationType type, Guid entityId);

    IDeferredQueryMultiple<Notification> GetUnreadByMemberId(Guid memberId);

    IDeferredQueryMultiple<Notification> GetUnreadByMemberId(Guid memberId, NotificationType type, Guid entityId);

    void MarkAsRead(IEnumerable<Notification> notifications);
}
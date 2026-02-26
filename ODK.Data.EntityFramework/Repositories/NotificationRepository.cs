using Microsoft.EntityFrameworkCore;
using ODK.Core.Notifications;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Notifications;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class NotificationRepository : ReadWriteRepositoryBase<Notification, INotificationQueryBuilder>, INotificationRepository
{
    public NotificationRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Notification> GetByMemberId(Guid memberId, Guid chapterId)
        => Query()
            .ForMember(memberId)
            .ForChapter(chapterId)
            .GetAll();

    public IDeferredQueryMultiple<Notification> GetUnreadByEntityId(NotificationType type, Guid entityId)
        => Query()
            .ForType(type)
            .ForEntity(entityId)
            .GetAll();

    public IDeferredQueryMultiple<Notification> GetUnreadByMemberId(Guid memberId)
        => Query()
            .ForMember(memberId)
            .Unread()
            .OrderByDescending(x => x.CreatedUtc)
            .GetAll();

    public IDeferredQueryMultiple<NotificationDto> GetUnreadDtosByMemberId(Guid memberId)
        => Query()
            .ForMember(memberId)
            .Unread()
            .ToDto()
            .OrderByDescending(x => x.Notification.CreatedUtc)
            .GetAll();

    public IDeferredQueryMultiple<Notification> GetUnreadByMemberId(Guid memberId, NotificationType type, Guid entityId)
        => Query()
            .ForMember(memberId)
            .Unread()
            .ForType(type)
            .ForEntity(entityId)
            .GetAll();

    public void MarkAsRead(IEnumerable<Notification> notifications)
    {
        var now = DateTime.UtcNow;
        foreach (var notification in notifications)
        {
            notification.ReadUtc = now;
        }

        UpdateMany(notifications);
    }

    public override INotificationQueryBuilder Query() => CreateQueryBuilder<INotificationQueryBuilder, Notification>(
        context => new NotificationQueryBuilder(context));
}
using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.Repositories;

public class NotificationRepository : ReadWriteRepositoryBase<Notification>, INotificationRepository
{
    public NotificationRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Notification> GetByMemberId(Guid memberId, Guid chapterId) => Set()
        .Where(x => x.MemberId == memberId && x.ChapterId == chapterId)
        .DeferredMultiple();

    public IDeferredQuery<int> GetCountByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredCount();

    public IDeferredQueryMultiple<Notification> GetUnreadByChapterId(Guid chapterId, NotificationType type, Guid entityId)
    {
        var query =
            from notification in Set()
            from member in Set<Member>().InChapter(chapterId)
            where notification.MemberId == member.Id
                && notification.Type == type
                && notification.EntityId == entityId
            select notification;

        return query.DeferredMultiple();
    }

    public IDeferredQueryMultiple<Notification> GetUnreadByMemberId(Guid memberId) => Set()
        .Where(x =>
            x.MemberId == memberId &&
            x.ReadUtc == null &&
            (x.ExpiresUtc == null || x.ExpiresUtc > DateTime.UtcNow))
        .OrderByDescending(x => x.CreatedUtc)
        .DeferredMultiple();

    public IDeferredQueryMultiple<Notification> GetUnreadByMemberId(Guid memberId, NotificationType type, Guid entityId) => Set()
        .Where(x =>
            x.MemberId == memberId &&
            x.ReadUtc == null &&
            x.Type == type &&
            x.EntityId == entityId &&
            (x.ExpiresUtc == null || x.ExpiresUtc > DateTime.UtcNow))
        .DeferredMultiple();

    public void MarkAsRead(IEnumerable<Notification> notifications)
    {
        var now = DateTime.UtcNow;
        foreach (var notification in notifications)
        {
            notification.ReadUtc = now;
        }

        UpdateMany(notifications);
    }

    public override void Delete(Notification entity)
    {
        var clone = entity.Clone();
        clone.Chapter = null;

        base.Delete(clone);
    }

    protected override IQueryable<Notification> Set() => base.Set()
        .Include(x => x.Chapter);

    public override void Update(Notification entity)
    {
        var clone = entity.Clone();
        clone.Chapter = null;

        base.Update(clone);
    }
}
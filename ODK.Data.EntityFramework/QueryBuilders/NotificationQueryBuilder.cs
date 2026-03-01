using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Data.Core.Notifications;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class NotificationQueryBuilder : DatabaseEntityQueryBuilder<Notification, INotificationQueryBuilder>, INotificationQueryBuilder
{
    public NotificationQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override INotificationQueryBuilder Builder => this;

    public INotificationQueryBuilder ForChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }

    public INotificationQueryBuilder ForEntity(Guid entityId)
    {
        Query = Query.Where(x => x.EntityId == entityId);
        return this;
    }

    public INotificationQueryBuilder ForMember(Guid memberId)
    {
        Query =
            from notification in Query
            where notification.MemberId == memberId
            from settings in Set<MemberNotificationSettings>()
                .Where(x => x.MemberId == notification.MemberId && x.NotificationType == notification.Type)
                .DefaultIfEmpty()
            from memberChapter in Set<MemberChapter>()
                .Where(x => x.MemberId == notification.MemberId && x.ChapterId == notification.ChapterId)
                .DefaultIfEmpty()
            from chapterSettings in Set<MemberChapterNotificationSettings>()
                .Where(x => x.MemberChapterId == memberChapter.Id && x.NotificationType == notification.Type)
                .DefaultIfEmpty()
            where (settings == null || !settings.Disabled) && (chapterSettings == null || !chapterSettings.Disabled)
            select notification;
        return this;
    }

    public INotificationQueryBuilder ForType(NotificationType type)
    {
        Query = Query.Where(x => x.Type == type);
        return this;
    }

    public IQueryBuilder<NotificationDto> ToDto()
    {
        var query =
            from notification in Query
            from chapter in Set<Chapter>()
                .Where(x => x.Id == notification.ChapterId)
                .DefaultIfEmpty()
            select new NotificationDto
            {
                Chapter = chapter,
                Notification = notification
            };

        return ProjectTo(query);
    }

    public INotificationQueryBuilder Unread()
    {
        Query = Query.Where(x => x.ReadUtc == null && (x.ExpiresUtc == null || x.ExpiresUtc > DateTime.UtcNow));
        return this;
    }
}
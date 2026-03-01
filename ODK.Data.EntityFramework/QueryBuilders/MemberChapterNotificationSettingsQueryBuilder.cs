using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class MemberChapterNotificationSettingsQueryBuilder
    : DatabaseEntityQueryBuilder<MemberChapterNotificationSettings, IMemberChapterNotificationSettingsQueryBuilder>, IMemberChapterNotificationSettingsQueryBuilder
{
    public MemberChapterNotificationSettingsQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override IMemberChapterNotificationSettingsQueryBuilder Builder => this;

    public IMemberChapterNotificationSettingsQueryBuilder ForChapter(Guid chapterId)
    {
        Query =
            from memberChapterNotificationSettings in Query
            from memberChapter in Set<MemberChapter>()
                .Where(x => x.Id == memberChapterNotificationSettings.MemberChapterId)
            where memberChapter.ChapterId == chapterId
            select memberChapterNotificationSettings;
        return this;
    }

    public IMemberChapterNotificationSettingsQueryBuilder ForGroup(NotificationGroupType group)
    {
        var types = group.Types();
        Query = Query.Where(x => types.Contains(x.NotificationType));
        return this;
    }

    public IMemberChapterNotificationSettingsQueryBuilder ForMember(Guid memberId)
    {
        Query =
            from settings in Query
            from memberChapter in Set<MemberChapter>()
                .Where(x => x.Id == settings.MemberChapterId)
            where memberChapter.MemberId == memberId
            select settings;
        return this;
    }

    public IQueryBuilder<MemberChapterNotificationSettingsDto> ToDto()
    {
        var query =
            from settings in Query
            from memberChapter in Set<MemberChapter>()
                .Where(x => x.Id == settings.MemberChapterId)
            from chapter in Set<Chapter>()
                .Where(x => x.Id == memberChapter.ChapterId)
            select new MemberChapterNotificationSettingsDto
            {
                MemberChapter = new MemberChapterDto
                {
                    Chapter = chapter,
                    MemberChapter = memberChapter
                },
                Settings = settings
            };

        return ProjectTo(query);
    }
}
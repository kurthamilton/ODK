using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Data.Core.Members;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberChapterNotificationSettingsQueryBuilder
    : IDatabaseEntityQueryBuilder<MemberChapterNotificationSettings, IMemberChapterNotificationSettingsQueryBuilder>
{
    IMemberChapterNotificationSettingsQueryBuilder ForChapter(Guid chapterId);

    IMemberChapterNotificationSettingsQueryBuilder ForGroup(NotificationGroupType group);

    IMemberChapterNotificationSettingsQueryBuilder ForMember(Guid memberId);

    IQueryBuilder<MemberChapterNotificationSettingsDto> ToDto();
}
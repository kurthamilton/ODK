using ODK.Core.Members;
using ODK.Data.Core.Members;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberChapterNotificationSettingsQueryBuilder
    : IDatabaseEntityQueryBuilder<MemberChapterNotificationSettings, IMemberChapterNotificationSettingsQueryBuilder>
{
    IMemberChapterNotificationSettingsQueryBuilder ForMember(Guid memberId);

    IQueryBuilder<MemberChapterNotificationSettingsDto> ToDto();
}
using ODK.Core.Members;
using ODK.Core.Notifications;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberNotificationSettingsQueryBuilder
    : IDatabaseEntityQueryBuilder<MemberNotificationSettings, IMemberNotificationSettingsQueryBuilder>
{
    IMemberNotificationSettingsQueryBuilder ForGroup(NotificationGroupType group);

    IMemberNotificationSettingsQueryBuilder ForMember(Guid memberId);

    IMemberNotificationSettingsQueryBuilder ForType(NotificationType type);
}
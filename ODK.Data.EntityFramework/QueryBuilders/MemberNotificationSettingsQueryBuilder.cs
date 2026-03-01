using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class MemberNotificationSettingsQueryBuilder
    : DatabaseEntityQueryBuilder<MemberNotificationSettings, IMemberNotificationSettingsQueryBuilder>,
    IMemberNotificationSettingsQueryBuilder
{
    public MemberNotificationSettingsQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override IMemberNotificationSettingsQueryBuilder Builder => this;

    public IMemberNotificationSettingsQueryBuilder ForGroup(NotificationGroupType group)
    {
        var types = group.Types();
        Query = Query.Where(x => types.Contains(x.NotificationType));
        return this;
    }

    public IMemberNotificationSettingsQueryBuilder ForMember(Guid memberId)
    {
        Query = Query.Where(x => x.MemberId == memberId);
        return this;
    }

    public IMemberNotificationSettingsQueryBuilder ForType(NotificationType type)
    {
        Query = Query.Where(x => x.NotificationType == type);
        return this;
    }
}
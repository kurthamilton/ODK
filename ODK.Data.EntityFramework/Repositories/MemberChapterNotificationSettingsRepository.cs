using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberChapterNotificationSettingsRepository
    : ReadWriteRepositoryBase<MemberChapterNotificationSettings, IMemberChapterNotificationSettingsQueryBuilder>, IMemberChapterNotificationSettingsRepository
{
    public MemberChapterNotificationSettingsRepository(DbContext context)
        : base(context)
    {
    }

    public override IMemberChapterNotificationSettingsQueryBuilder Query()
        => CreateQueryBuilder(context => new MemberChapterNotificationSettingsQueryBuilder(context));
}
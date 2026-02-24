using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberNotificationSettingsRepository : WriteRepositoryBase<MemberNotificationSettings>, IMemberNotificationSettingsRepository
{
    public MemberNotificationSettingsRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<MemberNotificationSettings> GetByChapterId(
        Guid chapterId,
        NotificationType notificationType)
    {
        var query =
            from settings in Set()
            from member in Set<Member>().InChapter(chapterId)
            where settings.MemberId == member.Id
                && settings.NotificationType == notificationType
            select settings;

        return query.DeferredMultiple();
    }

    public IDeferredQueryMultiple<MemberNotificationSettings> GetByMemberId(Guid memberId)
        => Set()
            .Where(x => x.MemberId == memberId)
            .DeferredMultiple();

    public IDeferredQueryMultiple<MemberNotificationSettings> GetByMemberIds(
        IReadOnlyCollection<Guid> memberIds,
        NotificationType notificationType)
        => Set()
            .Where(x => memberIds.Contains(x.MemberId) && x.NotificationType == notificationType)
            .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<MemberNotificationSettings> GetByMemberId(
        Guid memberId,
        NotificationType notificationType)
        => Set()
            .Where(x => x.MemberId == memberId && x.NotificationType == notificationType)
            .DeferredSingleOrDefault();
}
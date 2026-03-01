using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IMemberNotificationSettingsRepository
    : IReadWriteRepository<MemberNotificationSettings, IMemberNotificationSettingsQueryBuilder>
{
    IDeferredQueryMultiple<MemberNotificationSettings> GetByChapterId(
        Guid chapterId,
        NotificationType notificationType);

    IDeferredQueryMultiple<MemberNotificationSettings> GetByMemberId(Guid memberId);

    IDeferredQueryMultiple<MemberNotificationSettings> GetByMemberIds(
        IReadOnlyCollection<Guid> memberIds,
        NotificationType notificationType);

    IDeferredQuerySingleOrDefault<MemberNotificationSettings> GetByMemberId(
        Guid memberId,
        NotificationType notificationType);
}
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberNotificationSettingsRepository : IWriteRepository<MemberNotificationSettings>
{
    public IDeferredQueryMultiple<MemberNotificationSettings> GetByChapterId(
        Guid chapterId,
        NotificationType notificationType);

    public IDeferredQueryMultiple<MemberNotificationSettings> GetByMemberId(Guid memberId);

    public IDeferredQuerySingleOrDefault<MemberNotificationSettings> GetByMemberId(
        Guid memberId,
        NotificationType notificationType);
}

using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberPaymentSettingsRepository : IWriteRepository<MemberPaymentSettings>
{
    public IDeferredQuerySingleOrDefault<MemberPaymentSettings> GetByChapterId(Guid chapterId);

    public IDeferredQuerySingleOrDefault<MemberPaymentSettings> GetByMemberId(Guid memberId);
}
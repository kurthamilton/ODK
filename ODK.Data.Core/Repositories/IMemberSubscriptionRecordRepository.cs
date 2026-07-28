using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberSubscriptionRecordRepository : IReadWriteRepository<MemberSubscriptionRecord>
{
    IDeferredQuerySingle<MemberSubscriptionRecord> GetByExternalId(string externalId);

    IDeferredQuerySingleOrDefault<MemberSubscriptionRecord> GetByExternalIdOrDefault(string externalId);

    IDeferredQuerySingleOrDefault<MemberSubscriptionRecord> GetByInitiatorIdOrDefault(string initiatorId);

    IDeferredQuerySingleOrDefault<MemberSubscriptionRecord> GetLatest(Guid memberId, Guid chapterId);

    /// <summary>
    /// Whether the member's latest subscription record for the chapter is a recurring subscription that
    /// hasn't been cancelled - i.e. it will auto-renew, so no expiry warning is needed.
    /// </summary>
    IDeferredQuery<bool> HasActiveRecurringSubscription(Guid memberId, Guid chapterId);
}

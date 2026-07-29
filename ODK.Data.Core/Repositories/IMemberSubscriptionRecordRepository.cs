using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberSubscriptionRecordRepository : IReadWriteRepository<MemberSubscriptionRecord>
{
    IDeferredQuerySingleOrDefault<MemberSubscriptionRecord> GetByInitiatorIdOrDefault(string initiatorId);

    /// <summary>
    /// The member's current subscription record for the chapter (the one flagged <see cref="MemberSubscriptionRecord.IsCurrent"/>),
    /// or null if none. A single filtered-index seek.
    /// </summary>
    IDeferredQuerySingleOrDefault<MemberSubscriptionRecord> GetCurrentOrDefault(Guid memberId, Guid chapterId);

    IDeferredQuerySingleOrDefault<MemberSubscriptionRecord> GetLatest(Guid memberId, Guid chapterId);

    /// <summary>
    /// The most recent record for an external (payment provider) subscription id. Renewals append a record
    /// per event, all sharing the subscription id, so this returns the current one - used when cancelling.
    /// </summary>
    IDeferredQuerySingleOrDefault<MemberSubscriptionRecord> GetLatestByExternalIdOrDefault(string externalId);

    /// <summary>
    /// Whether the member's latest subscription record for the chapter is a recurring subscription that
    /// hasn't been cancelled - i.e. it will auto-renew, so no expiry warning is needed.
    /// </summary>
    IDeferredQuery<bool> HasActiveRecurringSubscription(Guid memberId, Guid chapterId);
}

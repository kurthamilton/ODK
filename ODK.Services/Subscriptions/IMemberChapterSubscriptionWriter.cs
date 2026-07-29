using ODK.Core.Members;

namespace ODK.Services.Subscriptions;

/// <summary>
/// The single place a member's current chapter subscription is written. It dual-writes both stores that hold
/// it - the MemberSubscriptionLog (source of truth) and the legacy MemberSubscriptions snapshot - so every
/// path that changes a subscription keeps them in sync. When the snapshot is retired, only this changes.
/// </summary>
public interface IMemberChapterSubscriptionWriter
{
    /// <summary>
    /// Makes <paramref name="newRecord"/> the member's current subscription record for the chapter: flags it
    /// current, clears the flag on the previously-current record (<paramref name="existingCurrent"/>), and
    /// upserts the snapshot (<paramref name="existingSnapshot"/>) to match. The writes are registered on the
    /// unit of work but not committed - the caller saves, so they stay atomic with the caller's other changes.
    /// </summary>
    void MakeRecordCurrent(
        MemberChapter memberChapter,
        MemberSubscriptionRecord newRecord,
        MemberSubscriptionRecord? existingCurrent,
        MemberSubscription? existingSnapshot);
}

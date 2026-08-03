using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Subscriptions;

/// <summary>
/// The single place a member's current site subscription is written. It dual-writes both stores that hold
/// it - the MemberSiteSubscriptionLog (the source of truth) and the legacy MemberSiteSubscriptions snapshot -
/// so every path that changes a subscription keeps them in sync. When the snapshot is retired, only this changes.
/// </summary>
public interface IMemberSiteSubscriptionWriter
{
    /// <summary>
    /// Loads the member's existing current record and snapshot itself, then makes <paramref name="newRecord"/>
    /// current (see the other overload). Use this where the caller hasn't already loaded them - it guarantees
    /// any previously-current record is flipped and the snapshot is upserted rather than duplicated, so a
    /// caller can't leave a stale current row behind by passing a null it never checked.
    /// </summary>
    Task MakeRecordCurrent(MemberSiteSubscriptionRecord newRecord, PlatformType platform);

    /// <summary>
    /// Makes <paramref name="newRecord"/> the member's current subscription record: flags it current, clears
    /// the flag on the previously-current record (<paramref name="existingCurrent"/>), and upserts the
    /// snapshot (<paramref name="existingSnapshot"/>) to match. Registered on the unit of work but not
    /// committed - the caller saves, so it stays atomic with the caller's other changes.
    /// </summary>
    void MakeRecordCurrent(
        MemberSiteSubscriptionRecord newRecord,
        MemberSiteSubscriptionRecord? existingCurrent,
        MemberSiteSubscription? existingSnapshot);
}

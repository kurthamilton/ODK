using ODK.Core.Members;

namespace ODK.Services.Subscriptions;

/// <summary>
/// The single place a member's current site subscription is written: it appends the new current
/// <see cref="MemberSiteSubscriptionRecord"/> to the MemberSiteSubscriptionLog and clears the flag on the
/// previously-current record.
/// </summary>
public interface IMemberSiteSubscriptionWriter
{
    /// <summary>
    /// Loads the member's existing current record itself, then makes <paramref name="newRecord"/> current
    /// (see the other overload). Use this where the caller hasn't already loaded it - it guarantees any
    /// previously-current record is flipped, so a caller can't leave a stale current row behind by passing a
    /// null it never checked.
    /// </summary>
    Task MakeRecordCurrent(MemberSiteSubscriptionRecord newRecord);

    /// <summary>
    /// Makes <paramref name="newRecord"/> the member's current subscription record: flags it current and
    /// clears the flag on the previously-current record (<paramref name="existingCurrent"/>). Registered on
    /// the unit of work but not committed - the caller saves, so it stays atomic with the caller's other
    /// changes.
    /// </summary>
    void MakeRecordCurrent(MemberSiteSubscriptionRecord newRecord, MemberSiteSubscriptionRecord? existingCurrent);
}

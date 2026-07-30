using ODK.Core.Members;

namespace ODK.Services.Subscriptions;

/// <summary>
/// The single place a member's current chapter subscription is written, to the MemberSubscriptionLog (the
/// source of truth). Centralised so every path that changes a subscription flips "current" consistently.
/// </summary>
public interface IMemberChapterSubscriptionWriter
{
    /// <summary>
    /// Makes <paramref name="newRecord"/> the member's current subscription record for the chapter: flags it
    /// current and clears the flag on the previously-current record (<paramref name="existingCurrent"/>). The
    /// writes are registered on the unit of work but not committed - the caller saves, so they stay atomic
    /// with the caller's other changes.
    /// </summary>
    void MakeRecordCurrent(
        MemberSubscriptionRecord newRecord,
        MemberSubscriptionRecord? existingCurrent);
}

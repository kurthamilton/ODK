namespace ODK.Services.Members.Models;

public static class MemberImportRowStatusExtensions
{
    /// <summary>
    /// Whether a row results in an invite, and so consumes a slot against the group owner's member limit.
    /// A new address and an existing member from outside the group both do; a member already in the group
    /// and an address that cannot be emailed are both skipped.
    /// </summary>
    public static bool IsImportable(this MemberImportRowStatus status)
        => status is MemberImportRowStatus.New or MemberImportRowStatus.ExistingNotInGroup;
}

namespace ODK.Services.Members.Models;

public static class MemberImportRowStatusExtensions
{
    /// <summary>
    /// Whether a row results in an invite, and so takes one of the group's remaining places. A new address
    /// and an existing member from outside the group both do; a member the group has already invited or
    /// already holds, and an address that cannot be emailed, are all skipped.
    /// </summary>
    public static bool IsImportable(this MemberImportRowStatus status)
        => status is MemberImportRowStatus.New or MemberImportRowStatus.ExistingNotInGroup;
}

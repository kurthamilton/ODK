namespace ODK.Services.Members.Models;

public enum MemberImportRowStatus
{
    /// <summary>
    /// Email address does not belong to any existing member. A new member will be created.
    /// </summary>
    New = 0,

    /// <summary>
    /// Email address belongs to an existing member who is not currently in this group. They will be added.
    /// </summary>
    ExistingNotInGroup = 1,

    /// <summary>
    /// Email address belongs to an existing member who is already in this group. The row will be skipped.
    /// </summary>
    ExistingInGroup = 2
}

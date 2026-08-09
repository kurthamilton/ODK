namespace ODK.Services.Members.Models;

public enum MemberImportRowStatus
{
    None,

    /// <summary>
    /// Email address does not belong to any existing member. A new member will be created.
    /// </summary>
    New,

    /// <summary>
    /// Email address belongs to an existing member who is not currently in this group. They will be added.
    /// </summary>
    ExistingNotInGroup,

    /// <summary>
    /// Email address belongs to an existing member who is already in this group. The row will be skipped.
    /// </summary>
    ExistingInGroup,

    /// <summary>
    /// Email address is not a valid format. The row will be skipped - importing it would create a member
    /// who can never be emailed, and the invite would bounce.
    /// </summary>
    Invalid
}

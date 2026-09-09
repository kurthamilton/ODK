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
    /// Email address belongs to a member the group has already invited. The row will be skipped - the
    /// outstanding invite is the ask, and raising a second one would neither reach them again nor take
    /// another of the group's places.
    /// </summary>
    AlreadyInvited,

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

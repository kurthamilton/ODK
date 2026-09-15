﻿namespace ODK.Services.Chapters.ViewModels;

/// <summary>
/// Who is looking at a group's moved page, as far as the page can tell, which is what decides its call to
/// action. The page is public, so an anonymous visitor is genuinely unknown rather than not yet loaded.
/// </summary>
/// <remarks>
/// Implicit values: this never leaves the process.
/// </remarks>
public enum GroupMovedVisitorState
{
    None,

    /// <summary>Nobody we can name. Offered the group, and a way back in if they were invited.</summary>
    Anonymous,

    /// <summary>Signed in, with no membership and no invite outstanding. Offered the group.</summary>
    SignedInNotMember,

    /// <summary>
    /// Signed in, with an invite this group has outstanding for them. Sent to accept it rather than to the
    /// join form: the group has already said yes, so asking them to apply is asking twice.
    /// </summary>
    Invited,

    /// <summary>Signed in and already in the group. Sent straight to it.</summary>
    Member
}

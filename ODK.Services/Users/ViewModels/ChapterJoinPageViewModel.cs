using ODK.Core.Chapters;

namespace ODK.Services.Users.ViewModels;

public class ChapterJoinPageViewModel
{
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// True when the invitation names a member who already has an account. Accepting it is then a matter of
    /// signing in and answering the group's questions rather than signing up, so the page asks them to sign in -
    /// a sign-up form could only tell them the address is already taken, and leave the invitation outstanding.
    /// </summary>
    public required bool InvitedMemberHasAccount { get; init; }

    /// <summary>
    /// Pre-filled from the invitation where there is one, so an invited member sees the details their group
    /// already holds rather than an empty form. Editable: the import supplied them, and they may be wrong. It
    /// also carries the invitation token the form posts back.
    /// </summary>
    public required PersonalDetailsFormViewModel PersonalDetails { get; init; }

    public required ChapterProfileFormViewModel Profile { get; init; }

    public required ChapterTexts? Texts { get; init; }
}

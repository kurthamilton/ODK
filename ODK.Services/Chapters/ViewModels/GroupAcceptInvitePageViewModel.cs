using ODK.Core.Chapters;
using ODK.Services.Users.ViewModels;

namespace ODK.Services.Chapters.ViewModels;

public class GroupAcceptInvitePageViewModel : GroupPageViewModel
{
    /// <summary>
    /// Pre-filled from the invitation, so the member sees the name their group already holds rather than an
    /// empty form. Null when the token names no outstanding invitation to this group, which is the whole of
    /// what a dead link looks like - an expired one, one already accepted, or one for somewhere else.
    /// </summary>
    public required AcceptInviteFormViewModel? Form { get; init; }

    /// <summary>
    /// True when the invitation names a member who can already sign in. There is no account to raise, so the
    /// page asks them to sign in: signing in is what identifies them, and the join page takes it from there.
    /// </summary>
    public required bool InvitedMemberHasAccount { get; init; }

    public required IReadOnlyCollection<ChapterProperty> Properties { get; init; }

    public required IReadOnlyCollection<ChapterPropertyOption> PropertyOptions { get; init; }

    public required bool RegistrationOpen { get; init; }

    public required ChapterTexts? Texts { get; init; }
}

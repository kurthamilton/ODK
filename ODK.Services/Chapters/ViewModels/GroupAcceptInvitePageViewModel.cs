using ODK.Core.Chapters;
using ODK.Services.Users.ViewModels;

namespace ODK.Services.Chapters.ViewModels;

public class GroupAcceptInvitePageViewModel : GroupPageViewModel
{
    public required AcceptInviteFormViewModel? Form { get; init; }

    public required bool InvitedMemberHasAccount { get; init; }

    public required IReadOnlyCollection<ChapterProperty> Properties { get; init; }

    public required IReadOnlyCollection<ChapterPropertyOption> PropertyOptions { get; init; }

    public required bool RegistrationOpen { get; init; }

    public required ChapterTexts? Texts { get; init; }
}

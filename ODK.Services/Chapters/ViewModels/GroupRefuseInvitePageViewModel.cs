using ODK.Services.Users.ViewModels;

namespace ODK.Services.Chapters.ViewModels;

public class GroupRefuseInvitePageViewModel : GroupPageViewModel
{
    public required RefuseInviteFormViewModel? Form { get; init; }
}

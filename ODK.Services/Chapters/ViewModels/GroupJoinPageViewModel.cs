using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class GroupJoinPageViewModel : GroupPageViewModel
{
    public required ChapterMembershipSettings? MembershipSettings { get; init; }

    public required IReadOnlyCollection<ChapterProperty> Properties { get; init; }

    public required IReadOnlyCollection<ChapterPropertyOption> PropertyOptions { get; init; }

    public required bool RegistrationOpen { get; init; }

    public required ChapterTexts? Texts { get; init; }
}

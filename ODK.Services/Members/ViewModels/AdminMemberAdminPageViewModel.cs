using ODK.Core.Chapters;

namespace ODK.Services.Members.ViewModels;

public class AdminMemberAdminPageViewModel
{
    public required ChapterAdminMember AdminMember { get; init; }

    public required bool CanEditRole { get; init; }

    public required bool ReadOnly { get; init; }

    public required IReadOnlyCollection<ChapterAdminRole> RoleOptions { get; init; }
}
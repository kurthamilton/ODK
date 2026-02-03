using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin.Members;

public class AdminMemberFormViewModel : AdminMemberFormSubmitViewModel
{
    public required bool CanEditRole { get; init; }

    public required bool ReadOnly { get; init; }

    public required IReadOnlyCollection<ChapterAdminRole> RoleOptions { get; init; }
}
namespace ODK.Services.Members.ViewModels;

public class SiteAdminFlaggedMembersViewModel
{
    public required IReadOnlyCollection<SiteAdminFlaggedMembersRowViewModel> Rows { get; init; }
}

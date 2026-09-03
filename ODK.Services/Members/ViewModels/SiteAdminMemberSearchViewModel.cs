namespace ODK.Services.Members.ViewModels;

public class SiteAdminMemberSearchViewModel
{
    public required IReadOnlyCollection<SiteAdminMemberSearchRowViewModel> Rows { get; init; }

    /// <summary>The search the rows answer, echoed back so the form can show what was asked for.</summary>
    public required string? Search { get; init; }

    /// <summary>
    /// Set when the search matched more members than the page shows, so a missing member reads as "narrow
    /// the search" rather than "not a member".
    /// </summary>
    public required bool Truncated { get; init; }
}

namespace ODK.Services.Members.ViewModels;

public class SiteAdminFlaggedMembersViewModel
{
    public required IReadOnlyCollection<SiteAdminFlaggedMembersRowViewModel> Rows { get; init; }

    /// <summary>The viewing site admin's zone: a site-wide page has no chapter to fall back to.</summary>
    public required TimeZoneInfo TimeZone { get; init; }
}

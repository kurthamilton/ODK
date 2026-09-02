using ODK.Core.Platforms;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteAdminMembersViewModel
{
    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<SiteAdminMemberRowViewModel> Rows { get; init; }

    /// <summary>The viewing site admin's zone: a site-wide page has no chapter to fall back to.</summary>
    public required TimeZoneInfo TimeZone { get; init; }
}

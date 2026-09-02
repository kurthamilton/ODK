using ODK.Data.Core.Referrals;

namespace ODK.Services.Referrals.ViewModels;

public class ReferralCampaignsAdminPageViewModel
{
    public required IReadOnlyCollection<ReferralCampaignSummaryDto> Campaigns { get; init; }

    /// <summary>The viewing site admin's zone: a site-wide page has no chapter to fall back to.</summary>
    public required TimeZoneInfo TimeZone { get; init; }
}

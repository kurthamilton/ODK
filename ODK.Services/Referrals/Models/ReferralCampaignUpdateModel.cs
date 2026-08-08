namespace ODK.Services.Referrals.Models;

public class ReferralCampaignUpdateModel
{
    /// <summary>
    /// The expiry as a plain local date, or null for a campaign that never expires. The service resolves
    /// it to a UTC instant; the caller must not do that conversion itself.
    /// </summary>
    public required DateTime? ExpiresLocalDate { get; set; }

    public required string Description { get; set; }

    public required string EmailSubject { get; set; }

    public required string EmailText { get; set; }

    public required string Name { get; set; }
}

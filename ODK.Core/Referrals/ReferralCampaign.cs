namespace ODK.Core.Referrals;

/// <summary>
/// A named campaign that members send referrals for. Site-wide rather than chapter-scoped.
/// </summary>
public class ReferralCampaign : IDatabaseEntity
{
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// HTML shown to the member on the refer page, describing the campaign.
    /// </summary>
    public string DescriptionHtml { get; set; } = string.Empty;

    /// <summary>
    /// Subject line of the referral email. Plain text, not HTML.
    /// </summary>
    public string EmailSubject { get; set; } = string.Empty;

    /// <summary>
    /// HTML body of the referral email. Interpolated with the member.fullName, url and referral.id tokens.
    /// </summary>
    public string EmailTextHtml { get; set; } = string.Empty;

    /// <summary>
    /// When the campaign stops accepting referrals; null means it never expires. Stored as the instant
    /// the expiry day ends, so a campaign expiring "on the 31st" is still open throughout the 31st.
    /// </summary>
    public DateTime? ExpiresUtc { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsExpired(DateTime utcNow) => ExpiresUtc != null && ExpiresUtc <= utcNow;
}

using ODK.Core.Members;

namespace ODK.Core.Referrals;

/// <summary>
/// One referral a member sent for a campaign. <see cref="CompletedUtc"/> is set when the referral is
/// fulfilled; what counts as fulfilment is not modelled yet.
/// </summary>
public class Referral : IDatabaseEntity, IMemberEntity
{
    public DateTime? CompletedUtc { get; set; }

    public DateTime CreatedUtc { get; set; }

    public string EmailAddress { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public Guid ReferralCampaignId { get; set; }
}

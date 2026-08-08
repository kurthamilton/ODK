using ODK.Core.Members;
using ODK.Core.Referrals;

namespace ODK.Data.Core.Referrals;

/// <summary>
/// A referral with the member who sent it, so the campaign page can show a name without a second query
/// per row.
/// </summary>
public class ReferralWithMemberDto
{
    public required Member Member { get; init; }

    public required Referral Referral { get; init; }
}

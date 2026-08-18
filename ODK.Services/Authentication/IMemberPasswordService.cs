using ODK.Core.Members;

namespace ODK.Services.Authentication;

/// <summary>
/// The password rules and the hashing behind them, shared by everything that sets one: activating an
/// account, resetting, changing, and the rehash a login performs when the stored hash is out of date.
/// </summary>
/// <remarks>
/// Its own type rather than private helpers, because a step on the account machine sets a password too, and
/// a second copy of the policy check is a divergence waiting to happen - a rule tightened in one place and
/// not the other is invisible until somebody sets a password the app should have refused.
/// </remarks>
public interface IMemberPasswordService
{
    /// <summary>
    /// Hashes <paramref name="password"/> onto the member's stored password, creating one where there is
    /// none. Whether the row is then added or updated is the caller's to decide - only the caller knows
    /// whether the member had one.
    /// </summary>
    MemberPassword Apply(MemberPassword? memberPassword, string password);

    /// <summary>
    /// Whether the password may be used: the length policy first, then whether it is known to have appeared
    /// in a breach. The failure message is written for the member.
    /// </summary>
    Task<ServiceResult> Validate(string password);
}

namespace ODK.Services.Authentication;

public interface IBreachedPasswordChecker
{
    /// <summary>
    /// Returns true if the password is known to have appeared in a data breach. Implementations must
    /// fail open (return false) when the check cannot be completed, so an outage never blocks sign-up.
    /// </summary>
    Task<bool> IsBreachedAsync(string password);
}

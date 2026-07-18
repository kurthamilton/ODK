namespace ODK.Services.Integrations.Authentication;

public class HibpBreachedPasswordCheckerSettings
{
    /// <summary>
    /// When false, the checker is a no-op (no external call). Config kill-switch so it can be disabled
    /// if the upstream service causes problems.
    /// </summary>
    public required bool Enabled { get; init; }

    /// <summary>
    /// Base URL of the Have I Been Pwned range API (k-anonymity endpoint), including trailing slash.
    /// </summary>
    public required string RangeApiUrl { get; init; }
}

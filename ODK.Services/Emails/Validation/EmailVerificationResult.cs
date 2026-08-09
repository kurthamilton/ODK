namespace ODK.Services.Emails.Validation;

public enum EmailVerificationResult
{
    None = 0,

    /// <summary>
    /// No usable answer - the provider errored, timed out, ran out of quota, or isn't configured. Treated
    /// as a pass: an unavailable verifier must never stop somebody signing up.
    /// </summary>
    Inconclusive = 1,

    /// <summary>The provider positively rejected the address. The only result that blocks.</summary>
    Invalid = 2,

    Valid = 3
}

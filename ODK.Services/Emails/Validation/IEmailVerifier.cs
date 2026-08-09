namespace ODK.Services.Emails.Validation;

/// <summary>
/// A third-party deliverability check, behind an interface so the provider can be swapped without the
/// callers knowing. Implementations live in ODK.Services.Integrations.
/// </summary>
/// <remarks>
/// Implementations must not throw: anything that isn't a definite verdict is
/// <see cref="EmailVerificationResult.Inconclusive"/>, so an outage or an exhausted quota degrades to the
/// format check rather than blocking the member.
/// </remarks>
public interface IEmailVerifier
{
    Task<EmailVerificationResult> Verify(string emailAddress);
}

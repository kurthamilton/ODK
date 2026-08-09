using System.Threading.Tasks;
using ODK.Services.Emails.Validation;

namespace ODK.Services.Tests.Helpers;

/// <summary>
/// A verifier that never reaches a verdict, mirroring the live behaviour when the provider is
/// unconfigured, down, or out of quota. Tests that aren't about verification use this so they exercise
/// the format check alone and never depend on an external service.
/// </summary>
internal class InconclusiveEmailVerifier : IEmailVerifier
{
    public Task<EmailVerificationResult> Verify(string emailAddress)
        => Task.FromResult(EmailVerificationResult.Inconclusive);
}

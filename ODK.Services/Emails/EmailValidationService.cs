using System.Text.RegularExpressions;
using ODK.Services.Emails.Validation;

namespace ODK.Services.Emails;

public class EmailValidationService : IEmailValidationService
{
    private static readonly Regex EmailAddressRegex = new(EmailAddressPattern.Value, RegexOptions.Compiled);

    private readonly IEmailVerifier _emailVerifier;

    public EmailValidationService(IEmailVerifier emailVerifier)
    {
        _emailVerifier = emailVerifier;
    }

    public async Task<ServiceResult> Validate(string emailAddress, EmailValidationLevel level)
    {
        if (string.IsNullOrWhiteSpace(emailAddress) || !EmailAddressRegex.IsMatch(emailAddress))
        {
            return ServiceResult.Failure("Invalid email address format");
        }

        // Tested for Full rather than against Soft, so an unset level costs no credit. Only a caller that
        // explicitly asks for the external check gets it.
        if (level != EmailValidationLevel.Full)
        {
            return ServiceResult.Successful();
        }

        // Only a positive rejection blocks. Inconclusive covers an outage, an exhausted quota, or no
        // configured provider, and must behave exactly like a pass - see IEmailVerifier.
        var verificationResult = await _emailVerifier.Verify(emailAddress);
        return verificationResult == EmailVerificationResult.Invalid
            ? ServiceResult.Failure("Email address could not be verified")
            : ServiceResult.Successful();
    }
}

using System.Text.RegularExpressions;

namespace ODK.Services.Emails;

public class EmailValidationService : IEmailValidationService
{
    // Deliberately permissive: a format check only, catching typos rather than proving deliverability.
    // Anything stricter belongs in the live check this service exists to make room for.
    private static readonly Regex EmailAddressRegex = new(
        "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", RegexOptions.Compiled);

    public Task<ServiceResult> Validate(string emailAddress)
        => Task.FromResult(EmailAddressRegex.IsMatch(emailAddress)
            ? ServiceResult.Successful()
            : ServiceResult.Failure("Invalid email address format"));
}

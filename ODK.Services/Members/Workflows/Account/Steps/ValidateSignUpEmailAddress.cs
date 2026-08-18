using ODK.Core.Workflows;
using ODK.Services.Emails;
using ODK.Services.Emails.Validation;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// The address is about to receive an activation email, and an unusable one leaves an account nobody can ever
/// activate - so it is checked before anything is written.
/// </summary>
public sealed class ValidateSignUpEmailAddress : IStep<AccountContext>
{
    private readonly IEmailValidationService _emailValidationService;

    public ValidateSignUpEmailAddress(IEmailValidationService emailValidationService)
    {
        _emailValidationService = emailValidationService;
    }

    public static string Description => "checks the email address";

    public static StepKind Kind => StepKind.Decision;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var result = await _emailValidationService.Validate(
            context.SignUpEmailAddress, EmailValidationLevel.Full);

        return result.Success
            ? StepOutcome.Continue()
            : StepOutcome.Fail(result.Message ?? string.Empty);
    }
}

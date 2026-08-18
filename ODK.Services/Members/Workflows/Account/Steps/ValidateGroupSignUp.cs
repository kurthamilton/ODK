using ODK.Core.Chapters;
using ODK.Core.Workflows;
using ODK.Services.Emails;
using ODK.Services.Emails.Validation;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// The group's own required questions, then the address. The questions come first because they cost nothing
/// to check, where verifying the address spends an external request an incomplete form need not pay for.
/// </summary>
public sealed class ValidateGroupSignUp : IStep<AccountContext>
{
    private readonly IEmailValidationService _emailValidationService;

    public ValidateGroupSignUp(IEmailValidationService emailValidationService)
    {
        _emailValidationService = emailValidationService;
    }

    public static string Description => "checks the group's required questions and the email address";

    public static StepKind Kind => StepKind.Decision;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var profile = context.RequiredProfile;

        var values = profile.Properties
            .ToDictionary(x => x.ChapterPropertyId, x => (string?)x.Value);

        var missing = context.ChapterProperties
            .GetMissingRequired(values, forApplication: true)
            .Select(x => x.GetDisplayText())
            .ToArray();

        if (missing.Length > 0)
        {
            return StepOutcome.Fail($"The following properties are required: {string.Join(", ", missing)}");
        }

        var emailResult = await _emailValidationService.Validate(profile.EmailAddress, EmailValidationLevel.Full);

        return emailResult.Success
            ? StepOutcome.Continue()
            : StepOutcome.Fail(emailResult.Message ?? "Invalid email address");
    }
}

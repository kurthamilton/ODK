using ODK.Core.Workflows;
using ODK.Services.Authentication;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Checks the password the member has chosen against the site's rules before anything is written. First on
/// the edge: a refused password must leave the account exactly as it was, still awaiting activation.
/// </summary>
public sealed class ValidateNewPassword : IStep<AccountContext>
{
    private readonly IMemberPasswordService _memberPasswordService;

    public ValidateNewPassword(IMemberPasswordService memberPasswordService)
    {
        _memberPasswordService = memberPasswordService;
    }

    public static string Description => "checks the password is allowed";

    public static StepKind Kind => StepKind.Decision;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var result = await _memberPasswordService.Validate(context.RequiredNewPassword);

        return result.Success
            ? StepOutcome.Continue()
            : StepOutcome.Fail(result.Message ?? string.Empty);
    }
}

using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Tells the address it already has an account. The sign-up reports success either way, so that nobody can
/// find out from the response whether an address is registered.
/// </summary>
public sealed class SendDuplicateMemberEmail : IStep<AccountContext>
{
    private readonly IMemberEmailService _memberEmailService;

    public SendDuplicateMemberEmail(IMemberEmailService memberEmailService)
    {
        _memberEmailService = memberEmailService;
    }

    public static string Description => "emails the address to say it already has an account";

    public static StepKind Kind => StepKind.ExternalEffect;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        await _memberEmailService.SendDuplicateMemberEmail(
            context.Request,
            context.Chapter,
            context.RequiredMember);

        return StepOutcome.Continue();
    }
}

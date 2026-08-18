using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Welcomes an account that arrived able to sign in, which is what an OAuth provider vouching for the address
/// produces. No activation email, because there is nothing left to prove.
/// </summary>
public sealed class SendSiteWelcomeEmail : IStep<AccountContext>
{
    private readonly IMemberEmailService _memberEmailService;

    public SendSiteWelcomeEmail(IMemberEmailService memberEmailService)
    {
        _memberEmailService = memberEmailService;
    }

    public static string Description => "emails a welcome";

    public static StepKind Kind => StepKind.ExternalEffect;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        await _memberEmailService.SendSiteWelcomeEmail(context.Request, context.RequiredAccount);
        return StepOutcome.Continue();
    }
}

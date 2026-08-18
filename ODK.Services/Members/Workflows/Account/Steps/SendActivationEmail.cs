using ODK.Core.Workflows;
using ODK.Services.Logging;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Emails the activation link. The account is already committed, so a failure to send is reported without
/// undoing it - they have an account and can be sent another link.
/// </summary>
public sealed class SendActivationEmail : IStep<AccountContext>
{
    private readonly ILoggingService _loggingService;
    private readonly IMemberEmailService _memberEmailService;

    public SendActivationEmail(IMemberEmailService memberEmailService, ILoggingService loggingService)
    {
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
    }

    public static string Description => "emails the activation link";

    public static StepKind Kind => StepKind.ExternalEffect;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        try
        {
            await _memberEmailService.SendActivationEmail(
                context.Request,
                context.Chapter,
                context.RequiredNewMember,
                context.RequiredActivationToken);

            return StepOutcome.Continue();
        }
        catch (Exception ex)
        {
            await _loggingService.Error("Error sending activation email", ex);
            return StepOutcome.Fail("Your account has been created but an error occurred sending an email.");
        }
    }
}

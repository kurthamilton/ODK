using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.ChapterMembership.Steps;

/// <summary>
/// Tells the member they are in. After the commit: the approval is already recorded, and an email cannot be
/// taken back.
/// </summary>
public sealed class SendMemberApprovedEmail : IStep<ChapterMembershipContext>
{
    private readonly IMemberEmailService _memberEmailService;

    public SendMemberApprovedEmail(IMemberEmailService memberEmailService)
    {
        _memberEmailService = memberEmailService;
    }

    public static string Description => "tells the member they are approved";

    public static StepKind Kind => StepKind.ExternalEffect;

    public async Task<StepOutcome> Execute(
        ChapterMembershipContext context, CancellationToken cancellationToken)
    {
        await _memberEmailService.SendMemberApprovedEmail(context.Request, context.Member);

        return StepOutcome.Continue();
    }
}

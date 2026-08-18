using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.ChapterMembership.Steps;

public sealed class SendNewMemberAdminEmail : IStep<ChapterMembershipContext>
{
    private readonly IMemberEmailService _memberEmailService;

    public SendNewMemberAdminEmail(IMemberEmailService memberEmailService)
    {
        _memberEmailService = memberEmailService;
    }

    public static string Description => "emails the group's admins";

    public static StepKind Kind => StepKind.ExternalEffect;

    public async Task<StepOutcome> Execute(ChapterMembershipContext context, CancellationToken cancellationToken)
    {
        await _memberEmailService.SendNewMemberAdminEmail(
            context.Request,
            context.AdminMembers,
            context.Member,
            context.ChapterProperties,
            context.MemberProperties);

        return StepOutcome.Continue();
    }
}

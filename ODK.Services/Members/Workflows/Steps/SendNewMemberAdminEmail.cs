using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Steps;

public sealed class SendNewMemberAdminEmail : IStep<AccountContext>
{
    private readonly IMemberEmailService _memberEmailService;

    public SendNewMemberAdminEmail(IMemberEmailService memberEmailService)
    {
        _memberEmailService = memberEmailService;
    }

    public static string Description => "emails the group's admins";

    public static StepKind Kind => StepKind.ExternalEffect;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        await _memberEmailService.SendNewMemberAdminEmail(
            context.Request,
            context.AdminMembers,
            context.RequiredMember,
            context.ChapterProperties,
            context.MemberProperties);

        return StepOutcome.Continue();
    }
}

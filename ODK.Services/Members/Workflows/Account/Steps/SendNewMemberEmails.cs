using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Emails the group's admins about their new member, with the answers they gave to the group's questions.
/// After the commit: the membership is already recorded, and an email cannot be taken back.
/// </summary>
public sealed class SendNewMemberEmails : IStep<AccountContext>
{
    private readonly IMemberEmailService _memberEmailService;

    public SendNewMemberEmails(IMemberEmailService memberEmailService)
    {
        _memberEmailService = memberEmailService;
    }

    public static string Description => "emails the group about its new member";

    public static StepKind Kind => StepKind.ExternalEffect;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        await _memberEmailService.SendNewMemberEmails(
            ChapterServiceRequest.Create(context.RequiredChapter, context.Request),
            context.AdminMembers,
            context.RequiredMember,
            context.ChapterProperties,
            context.MemberProperties);

        return StepOutcome.Continue();
    }
}

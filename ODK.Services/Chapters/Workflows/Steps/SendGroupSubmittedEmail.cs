using ODK.Core.Workflows;
using ODK.Services.Members;

namespace ODK.Services.Chapters.Workflows.Steps;

/// <summary>
/// Tells site admins a group is waiting on them. After the commit: the submission is already recorded, and
/// an email cannot be taken back.
/// </summary>
public sealed class SendGroupSubmittedEmail : IStep<ChapterPublicationContext>
{
    private readonly IMemberEmailService _memberEmailService;

    public SendGroupSubmittedEmail(IMemberEmailService memberEmailService)
    {
        _memberEmailService = memberEmailService;
    }

    public static string Description => "tells site admins it is waiting on them";

    public static StepKind Kind => StepKind.ExternalEffect;

    public async Task<StepOutcome> Execute(
        ChapterPublicationContext context, CancellationToken cancellationToken)
    {
        await _memberEmailService.SendGroupSubmittedEmail(
            context.Request,
            context.Chapter,
            context.RequiredSiteAdmins);

        return StepOutcome.Continue();
    }
}

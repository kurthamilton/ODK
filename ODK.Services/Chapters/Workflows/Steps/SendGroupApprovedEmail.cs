using ODK.Core.Workflows;
using ODK.Services.Members;

namespace ODK.Services.Chapters.Workflows.Steps;

/// <summary>
/// Tells the owner their group has been approved, so they know they can publish it. After the commit: the
/// approval is already recorded, and an email cannot be taken back.
/// </summary>
public sealed class SendGroupApprovedEmail : IStep<ChapterPublicationContext>
{
    private readonly IMemberEmailService _memberEmailService;

    public SendGroupApprovedEmail(IMemberEmailService memberEmailService)
    {
        _memberEmailService = memberEmailService;
    }

    public static string Description => "tells the owner it is approved";

    public static StepKind Kind => StepKind.ExternalEffect;

    public async Task<StepOutcome> Execute(
        ChapterPublicationContext context, CancellationToken cancellationToken)
    {
        await _memberEmailService.SendGroupApprovedEmail(
            ChapterServiceRequest.Create(context.Chapter, context.Request),
            context.RequiredOwner);

        return StepOutcome.Continue();
    }
}

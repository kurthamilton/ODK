using ODK.Core.Workflows;
using ODK.Services.Members;

namespace ODK.Services.Chapters.Workflows.Steps;

/// <summary>
/// Tells the owner that invites raised while the group was unpublished are waiting to be sent, now that
/// they have somewhere to land. After the commit, for the reason its counterpart on approval gives: the
/// publication is already recorded, and an email cannot be taken back.
/// </summary>
/// <remarks>
/// A group holding none sends nothing, which is every group that imported nothing before publishing.
/// Sending the invites themselves is the owner's own action, on the invited members page.
/// </remarks>
public sealed class SendInvitesWaitingEmail : IStep<ChapterPublicationContext>
{
    private readonly IMemberEmailService _memberEmailService;

    public SendInvitesWaitingEmail(IMemberEmailService memberEmailService)
    {
        _memberEmailService = memberEmailService;
    }

    public static string Description => "tells the owner it is holding invites";

    public static StepKind Kind => StepKind.ExternalEffect;

    public async Task<StepOutcome> Execute(
        ChapterPublicationContext context, CancellationToken cancellationToken)
    {
        var heldInvites = context.RequiredHeldInvites;
        if (heldInvites == 0)
        {
            return StepOutcome.Continue();
        }

        await _memberEmailService.SendInvitesWaitingEmail(
            ChapterServiceRequest.Create(context.Chapter, context.Request),
            context.RequiredOwner,
            heldInvites);

        return StepOutcome.Continue();
    }
}

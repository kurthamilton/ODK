using ODK.Core.Workflows;
using ODK.Services.Members;

namespace ODK.Services.Chapters.Workflows.Steps;

/// <summary>
/// Sends the invites the group raised while it was unpublished. After the commit, for the reason its
/// counterpart on approval gives: the publication is already recorded, and an email cannot be taken back.
/// </summary>
/// <remarks>
/// A group can prepare an import before anyone outside it can see it, so this is where those invites go out.
/// A group holding none sends nothing, which is every group that imported nothing before publishing.
/// </remarks>
public sealed class SendQueuedInvites : IStep<ChapterPublicationContext>
{
    private readonly IMemberAdminService _memberAdminService;

    public SendQueuedInvites(IMemberAdminService memberAdminService)
    {
        _memberAdminService = memberAdminService;
    }

    public static string Description => "sends the invites it was holding";

    public static StepKind Kind => StepKind.ExternalEffect;

    public async Task<StepOutcome> Execute(
        ChapterPublicationContext context, CancellationToken cancellationToken)
    {
        await _memberAdminService.SendQueuedInviteEmails(
            ChapterServiceRequest.Create(context.Chapter, context.Request));

        return StepOutcome.Continue();
    }
}

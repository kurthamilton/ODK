using ODK.Core.Workflows;
using ODK.Services.Members.Workflows.ChapterMembership;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Accepting an invitation joins the group, so this runs the membership machine as part of the acceptance.
/// Both machines stage their writes into the same unit of work and one commit follows, because an account
/// activated without the membership it was activated to reach would have accepted nothing.
/// </summary>
/// <remarks>
/// The membership machine's Accept transition stages writes and stops there - no commit, no email. The builder
/// cannot see inside this step to enforce that, so
/// <c>ChapterMembershipStateMachineTests.Create_Accept_StagesWritesAndNothingElse</c> does.
/// </remarks>
public sealed class AcceptTheInvitation : IStep<AccountContext>
{
    private readonly IChapterMembershipContextFactory _contextFactory;
    private readonly StateMachineRunner<
        ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext> _chapterMembershipWorkflow;

    public AcceptTheInvitation(
        IChapterMembershipContextFactory contextFactory,
        StateMachineRunner<ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext>
            chapterMembershipWorkflow)
    {
        _chapterMembershipWorkflow = chapterMembershipWorkflow;
        _contextFactory = contextFactory;
    }

    public static string Description => "joins the group the invitation was to";

    public static StepKind Kind => StepKind.Write;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var membershipContext = _contextFactory.CreateForAcceptInvite(context);

        var result = await _chapterMembershipWorkflow.Fire(
            ChapterMembershipTrigger.Accept,
            membershipContext,
            cancellationToken);

        return result.Success
            ? StepOutcome.Continue()
            : StepOutcome.Fail(result.Message ?? "The group could not be joined");
    }
}

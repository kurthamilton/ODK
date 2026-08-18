using ODK.Core.Workflows;
using ODK.Services.Members.Workflows.ChapterMembership;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Signing up to a group is joining it, so this runs the membership machine as part of the sign-up. Both
/// machines stage their writes into the same unit of work and one commit follows, because a member created
/// without their membership would be a member of nothing.
/// </summary>
/// <remarks>
/// The membership machine's SignUp transition stages writes and stops there - no commit, no email. The
/// builder cannot see inside this step to enforce that, so
/// <c>ChapterMembershipStateMachineTests.Create_SignUp_StagesWritesAndNothingElse</c> does.
/// </remarks>
public sealed class JoinTheGroup : IStep<AccountContext>
{
    private readonly IChapterMembershipContextFactory _contextFactory;
    private readonly StateMachineRunner<
        ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext> _chapterMembership;

    public JoinTheGroup(
        IChapterMembershipContextFactory contextFactory,
        StateMachineRunner<ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext>
            chapterMembership)
    {
        _chapterMembership = chapterMembership;
        _contextFactory = contextFactory;
    }

    public static string Description => "joins the group";

    public static StepKind Kind => StepKind.Write;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var membershipContext = _contextFactory.CreateForGroupSignUp(context);

        var result = await _chapterMembership.Fire(
            ChapterMembershipTrigger.SignUp,
            membershipContext,
            cancellationToken);

        return result.Success
            ? StepOutcome.Continue()
            : StepOutcome.Fail(result.Message ?? "The group could not be joined");
    }
}

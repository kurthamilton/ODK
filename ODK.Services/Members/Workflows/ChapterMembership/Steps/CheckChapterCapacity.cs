using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.ChapterMembership.Steps;

/// <summary>The group owner's subscription caps how many members the group can hold.</summary>
public sealed class CheckChapterCapacity : IStep<ChapterMembershipContext>
{
    public static string Description => "checks the group has room for another member";

    public static StepKind Kind => StepKind.Decision;

    public Task<StepOutcome> Execute(ChapterMembershipContext context, CancellationToken cancellationToken)
    {
        var outcome = context.OwnerSubscription?.HasCapacity(context.MemberCount) == true
            ? StepOutcome.Continue()
            : StepOutcome.Fail("This group is not able to welcome any new members");

        return Task.FromResult(outcome);
    }
}

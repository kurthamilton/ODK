using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// The group owner's subscription caps how many members the group can hold, and signing up to a group joins
/// it - so the cap applies here exactly as it does to a member who already has an account and joins.
/// </summary>
/// <remarks>
/// A decision, so a full group is refused before anything is written. The Join route enforces the same cap
/// through its own step against its own context; both ask <see cref="Core.Subscriptions.SiteSubscription"/>,
/// which is where the rule lives.
/// </remarks>
public sealed class CheckGroupCapacity : IStep<AccountContext>
{
    public static string Description => "checks the group has room for another member";

    public static StepKind Kind => StepKind.Decision;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var outcome = context.OwnerSubscription?.HasCapacity(context.MemberCount) == true
            ? StepOutcome.Continue()
            : StepOutcome.Fail("This group is not able to welcome any new members");

        return Task.FromResult(outcome);
    }
}

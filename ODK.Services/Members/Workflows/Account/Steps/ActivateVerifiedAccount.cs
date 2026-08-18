using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Marks the account able to sign in without an activation link. Only reached where an OAuth provider has
/// confirmed the address being registered belongs to the signer-up, which is everything an activation email
/// establishes - so the guard on the edge is the proof, and this records it.
/// </summary>
public sealed class ActivateVerifiedAccount : IStep<AccountContext>
{
    public static string Description => "marks the account activated";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        // Staged, not yet committed, so setting the property is the whole write.
        context.RequiredNewMember.Activated = true;

        return Task.FromResult(StepOutcome.Continue());
    }
}

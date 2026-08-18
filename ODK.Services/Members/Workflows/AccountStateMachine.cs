using ODK.Core.Platforms;
using ODK.Services.Members.Workflows.Guards;
using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows;

/// <summary>
/// Every route into an account and into a group, on both platforms. The transitions carry no steps
/// yet: this describes the graph the existing services already walk, and the work behind each edge
/// moves here as it is extracted from them.
/// </summary>
public static class AccountStateMachine
{
    public const string Name = "Account creation";

    public static StateMachineDefinition<AccountState, AccountTrigger, AccountContext> Create()
    {
        var approvalIsRequired = new ApprovalIsRequired();
        var onDrunkenKnitwits = new OnPlatform(PlatformType.DrunkenKnitwits);
        var onGroupSquirrel = new OnPlatform(PlatformType.Default);
        var presentedWithTheInviteToken = new InviteTokenMatches();
        var verifiedByOAuth = new SignUpIsVerifiedByOAuth();

        return StateMachine
            .Define<AccountState, AccountTrigger, AccountContext>(Name)
            .StartingAt(AccountState.Anonymous)
            .Transition(AccountState.Anonymous, AccountTrigger.Import, AccountState.Invited)
            .Transition(
                AccountState.Anonymous,
                AccountTrigger.SignUp,
                AccountState.Registered,
                x => x.When(Guard.Not(verifiedByOAuth)))
            .Transition(
                AccountState.Anonymous,
                AccountTrigger.SignUp,
                AccountState.Activated,
                x => x.When(verifiedByOAuth))

            /* Signing up against an address that already has an unactivated account. The account is
               discarded and recreated from the newly submitted details, carrying its activation token and
               its invitations across, so it ends where it started. */
            .Transition(AccountState.Registered, AccountTrigger.SignUp, AccountState.Registered)

            /* Signing up while invited. On Drunken Knitwits signing up is joining, so an invitation
               presented with its token lands in the group; without the token it is an ordinary sign-up that
               waits on an activation email. Group Squirrel's join page needs an account, so joining is a
               separate step there whatever the invitation says. */
            .Transition(
                AccountState.Invited,
                AccountTrigger.SignUp,
                AccountState.GroupMember,
                x => x.When(onDrunkenKnitwits).When(presentedWithTheInviteToken))
            .Transition(
                AccountState.Invited,
                AccountTrigger.SignUp,
                AccountState.Registered,
                x => x.When(onDrunkenKnitwits).When(Guard.Not(presentedWithTheInviteToken)))
            .Transition(
                AccountState.Invited,
                AccountTrigger.SignUp,
                AccountState.Registered,
                x => x.When(onGroupSquirrel))

            .Transition(AccountState.Registered, AccountTrigger.Activate, AccountState.Activated)

            /* An invitation is approval, so an invited member joining is never queued: the group asked
               them in. That is a property of the context rather than of the edge, which is why both edges
               turn on the one guard. */
            .Transition(
                AccountState.Activated,
                AccountTrigger.Join,
                AccountState.PendingApproval,
                x => x.When(approvalIsRequired))
            .Transition(
                AccountState.Activated,
                AccountTrigger.Join,
                AccountState.GroupMember,
                x => x.When(Guard.Not(approvalIsRequired)))

            .Transition(AccountState.PendingApproval, AccountTrigger.Approve, AccountState.GroupMember)
            .Build();
    }
}

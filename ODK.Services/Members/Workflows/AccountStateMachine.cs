using ODK.Core.Platforms;
using ODK.Core.Workflows;
using ODK.Services.Members.Workflows.Guards;
using ODK.Services.Members.Workflows.Steps;
using ODK.Services.Workflows;

namespace ODK.Services.Members.Workflows;

/// <summary>
/// Every route into an account and into a group, on both platforms. Only the Join transitions carry
/// steps so far; the rest describe the graph the existing services already walk, and the work behind
/// each edge moves here as it is extracted from them.
/// </summary>
public static class AccountStateMachine
{
    public const string Name = "Account creation";

    public static StateMachineDefinition<AccountState, AccountTrigger, AccountContext> Create()
    {
        var approvalIsRequired = new ApprovalIsRequired();
        var memberOfTheGroup = new IsMemberOfChapter();
        var membershipIsApproved = new MembershipIsApproved();
        var onDrunkenKnitwits = new OnPlatform(PlatformType.DrunkenKnitwits);
        var onGroupSquirrel = new OnPlatform(PlatformType.Default);
        var presentedWithTheInviteToken = new InviteTokenMatches();
        var verifiedByOAuth = new SignUpIsVerifiedByOAuth();

        /* Both Join edges do the same work and differ only in where they land - approval is written on the
           membership row, so the state follows from it. Shared rather than declared twice, so the two can
           never drift apart. */
        var join = (TransitionBuilder<AccountContext> x) => x
            .Then<CheckChapterCapacity>()
            .Then<CheckMemberProperties>()
            .Then<AddMemberToChapter>()
            .Then<ConsumeInvitation>()
            .Then<RaiseNewMemberNotifications>()
            .Then<Commit<AccountContext>>()
            .Then<SendNewMemberAdminEmail>();

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

            /* Signing up while invited. Drunken Knitwits writes the membership at sign-up, so both of its
               edges land short of being able to sign in - the token decides whether the group approves them
               and whether an activation email is sent, not where they end up. Group Squirrel's join page
               needs an account, so signing up there joins nothing and leaves the invitation outstanding. */
            .Transition(
                AccountState.Invited,
                AccountTrigger.SignUp,
                AccountState.Registered,
                x => x.When(onDrunkenKnitwits).When(presentedWithTheInviteToken))
            .Transition(
                AccountState.Invited,
                AccountTrigger.SignUp,
                AccountState.Registered,
                x => x.When(onDrunkenKnitwits).When(Guard.Not(presentedWithTheInviteToken)))
            .Transition(
                AccountState.Invited,
                AccountTrigger.SignUp,
                AccountState.Invited,
                x => x.When(onGroupSquirrel))

            .Transition(AccountState.Invited, AccountTrigger.Activate, AccountState.Activated)

            /* Activating settles what the account already holds. A member who signed up on Drunken Knitwits
               has a membership written before they could sign in, so activating lands them in the group at
               whatever approval it recorded; everyone else arrives belonging to no group. */
            .Transition(
                AccountState.Registered,
                AccountTrigger.Activate,
                AccountState.Activated,
                x => x.When(Guard.Not(memberOfTheGroup)))
            .Transition(
                AccountState.Registered,
                AccountTrigger.Activate,
                AccountState.GroupMember,
                x => x.When(memberOfTheGroup).When(membershipIsApproved))
            .Transition(
                AccountState.Registered,
                AccountTrigger.Activate,
                AccountState.PendingApproval,
                x => x.When(memberOfTheGroup).When(Guard.Not(membershipIsApproved)))

            /* An invitation is approval, so an invited member joining is never queued: the group asked
               them in. That is a property of the context rather than of the edge, which is why both edges
               turn on the one guard. */
            .Transition(
                AccountState.Activated,
                AccountTrigger.Join,
                AccountState.PendingApproval,
                x => join(x.When(approvalIsRequired)))
            .Transition(
                AccountState.Activated,
                AccountTrigger.Join,
                AccountState.GroupMember,
                x => join(x.When(Guard.Not(approvalIsRequired))))

            .Transition(AccountState.PendingApproval, AccountTrigger.Approve, AccountState.GroupMember)
            .Build();
    }
}

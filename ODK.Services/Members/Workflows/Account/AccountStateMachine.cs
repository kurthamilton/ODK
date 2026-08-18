using ODK.Core.Workflows;
using ODK.Services.Members.Workflows.Account.Guards;
using ODK.Services.Members.Workflows.Account.Steps;
using ODK.Services.Workflows;

namespace ODK.Services.Members.Workflows.Account;

/// <summary>
/// Every route to an account that can sign in. The group sign-up transitions carry steps; the rest describe
/// the graph the existing services already walk, and the work behind each edge moves here as it is extracted.
/// </summary>
public static class AccountStateMachine
{
    public const string Name = "Account";

    public static StateMachineDefinition<AccountState, AccountTrigger, AccountContext> Create()
    {
        var presentedWithTheInviteToken = new InviteTokenMatches();
        var toAGroup = new SignUpIsToAGroup();
        var verifiedByOAuth = new SignUpIsVerifiedByOAuth();

        /* Creating the account a group sign-up asks for. Shared by the edge that has no account to start from
           and the two that discard an unactivated one, so the three cannot drift apart. The membership is
           written by JoinTheGroup part-way through, before the single commit, because an account created
           without its membership would belong to no group. */
        var createGroupAccount = (TransitionBuilder<AccountContext> x) => x
            .Then<CreateMember>()
            .Then<ApplyEmailOptIn>()
            .Then<CreateMemberPreferences>()
            .Then<JoinTheGroup>()
            .Then<AddMemberLocationFromChapter>()
            .Then<MakeSiteSubscriptionCurrent>()
            .Then<StoreAvatar>()
            .Then<IssueActivationToken>()
            .Then<CarryOverInvitations>()
            .Then<CommitSignUp>();

        /* Creating the account a sign-up to the site asks for. Shared by the edge with no account to start from
           and the one that discards an unactivated one. What follows differs: an account an OAuth provider
           vouched for can sign in already, so it gets a welcome instead of an activation link and no token. */
        var createSiteAccount = (TransitionBuilder<AccountContext> x) => x
            .Then<CreateSiteMember>()
            .Then<MakeSiteSubscriptionCurrent>()
            .Then<AddMemberTopics>()
            .Then<CarryOverInvitations>();

        return StateMachine
            .Define<AccountState, AccountTrigger, AccountContext>(Name)
            .StartingAt(AccountState.Anonymous)

            /* An import raises the account, so the address has one before anybody signs up against it. It cannot
               sign in until an activation link is followed, which is why this lands where a sign-up does. */
            /* Write steps only, and no commit: an import is a batch, and the caller commits the whole file at
               once. The invitation is the other machine's business, raised alongside this. */
            .Transition(
                AccountState.Anonymous,
                AccountTrigger.Import,
                AccountState.Registered,
                x => x.Then<CreateImportedMember>())

            /* Signing up to a group, with no account to start from. There is no invitation to present - an
               invited address already has the account an import raised, so it starts from Registered below. */
            .Transition(
                AccountState.Anonymous,
                AccountTrigger.SignUp,
                AccountState.Registered,
                x => createGroupAccount(x
                        .When(toAGroup)
                        .Then<CheckGroupCapacity>()
                        .Then<ValidateGroupSignUp>()
                        .Then<ValidateSignUpImage>())
                    .Then<SendActivationEmail>())

            .Transition(
                AccountState.Anonymous,
                AccountTrigger.SignUp,
                AccountState.Registered,
                x => createSiteAccount(x
                        .When(Guard.Not(toAGroup))
                        .When(Guard.Not(verifiedByOAuth))
                        .Then<ValidateSignUpEmailAddress>())
                    .Then<IssueActivationToken>()
                    .Then<CommitSignUp>()
                    .Then<AddNewMemberTopics>()
                    .Then<SendActivationEmail>())

            /* An OAuth provider confirming the address is everything an activation email establishes, so the
               account arrives able to sign in. */
            .Transition(
                AccountState.Anonymous,
                AccountTrigger.SignUp,
                AccountState.Activated,
                x => createSiteAccount(x
                        .When(Guard.Not(toAGroup))
                        .When(verifiedByOAuth)
                        .Then<ValidateSignUpEmailAddress>())
                    .Then<ActivateVerifiedAccount>()
                    .Then<CommitSignUp>()
                    .Then<AddNewMemberTopics>()
                    .Then<SendSiteWelcomeEmail>())

            /* Signing up against an address that already has an unactivated account: it is discarded and
               recreated from the details just submitted, so the latest of them wins, and it ends where it
               started. Presenting the invitation's token proves the sign-up reached that inbox, which is what
               an activation email would have established - so that edge sends none, and the caller hands them
               straight to setting a password. */
            .Transition(
                AccountState.Registered,
                AccountTrigger.SignUp,
                AccountState.Registered,
                x => createGroupAccount(x
                    .When(toAGroup)
                    .When(presentedWithTheInviteToken)
                    .Then<CheckGroupCapacity>()
                    .Then<ValidateGroupSignUp>()
                    .Then<ValidateSignUpImage>()
                    .Then<DiscardUnactivatedAccount>()
                    .Then<Commit<AccountContext>>()))

            .Transition(
                AccountState.Registered,
                AccountTrigger.SignUp,
                AccountState.Registered,
                x => createGroupAccount(x
                        .When(toAGroup)
                        .When(Guard.Not(presentedWithTheInviteToken))
                        .Then<CheckGroupCapacity>()
                        .Then<ValidateGroupSignUp>()
                        .Then<ValidateSignUpImage>()
                        .Then<DiscardUnactivatedAccount>()
                        .Then<Commit<AccountContext>>())
                    .Then<SendActivationEmail>())

            .Transition(
                AccountState.Registered,
                AccountTrigger.SignUp,
                AccountState.Registered,
                x => createSiteAccount(x
                        .When(Guard.Not(toAGroup))
                        .Then<ValidateSignUpEmailAddress>()
                        .Then<DiscardUnactivatedAccount>()
                        .Then<Commit<AccountContext>>())
                    .Then<IssueActivationToken>()
                    .Then<CommitSignUp>()
                    .Then<AddNewMemberTopics>()
                    .Then<SendActivationEmail>())

            /* Signing up against an address that already has an account it can sign in with. Nothing is
               created - the address is emailed to say so, and the sign-up reports success either way, so that
               nobody can find out from the response whether an address is registered. */
            .Transition(
                AccountState.Activated,
                AccountTrigger.SignUp,
                AccountState.Activated,
                x => x.Then<SendDuplicateMemberEmail>())

            .Transition(AccountState.Registered, AccountTrigger.Activate, AccountState.Activated)
            .Build();
    }
}

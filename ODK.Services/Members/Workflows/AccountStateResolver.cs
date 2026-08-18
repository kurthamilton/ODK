using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows;

/// <summary>
/// Derives the account state from the domain. Nothing stores it, so this is the only thing that decides
/// what state a member is in.
/// </summary>
public sealed class AccountStateResolver : IStateResolver<AccountState, AccountContext>
{
    public AccountState Resolve(AccountContext context)
    {
        var member = context.Member;
        if (member == null)
        {
            return AccountState.Anonymous;
        }

        /* Being unable to sign in outranks every row a member already has. Drunken Knitwits writes the
           membership at sign-up, well before activation, so a member can hold one while still unable to act -
           and this ordering is what decides that they are Registered rather than a member of the group. */
        if (!member.Activated)
        {
            return context.Invite != null ? AccountState.Invited : AccountState.Registered;
        }

        var memberChapter = member.MemberChapter(context.ChapterId);
        if (memberChapter == null)
        {
            return AccountState.Activated;
        }

        return memberChapter.Approved ? AccountState.GroupMember : AccountState.PendingApproval;
    }
}

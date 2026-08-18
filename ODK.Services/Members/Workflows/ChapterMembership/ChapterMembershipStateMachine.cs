using ODK.Core.Workflows;
using ODK.Services.Members.Workflows.ChapterMembership.Guards;
using ODK.Services.Members.Workflows.ChapterMembership.Steps;
using ODK.Services.Workflows;

namespace ODK.Services.Members.Workflows.ChapterMembership;

/// <summary>
/// Every route into a group. Only the Join transitions carry steps so far; the rest describe the graph the
/// existing services already walk, and the work behind each edge moves here as it is extracted.
/// </summary>
public static class ChapterMembershipStateMachine
{
    public const string Name = "Chapter membership";

    public static StateMachineDefinition<
        ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext> Create()
    {
        var approvalIsRequired = new ApprovalIsRequired();

        /* Signing up to a group joins it. Writes only: the account machine's SignUp transition runs this one as
           a step and owns the commit and the emails, because the account and the membership go in together. No
           capacity check and nobody notified - the group hears about a new member when the account is
           activated. */
        var signUp = (TransitionBuilder<ChapterMembershipContext> x) => x.Then<AddMemberToChapter>();

        /* Every Join edge does the same work and differs only in where it lands - approval is written on the
           membership row, so the state follows from it. Shared rather than declared three times, so they can
           never drift apart. */
        var join = (TransitionBuilder<ChapterMembershipContext> x) => x
            .Then<CheckChapterCapacity>()
            .Then<CheckMemberProperties>()
            .Then<AddMemberToChapter>()
            .Then<ConsumeInvitation>()
            .Then<RaiseNewMemberNotifications>()
            .Then<Commit<ChapterMembershipContext>>()
            .Then<SendNewMemberAdminEmail>();

        return StateMachine
            .Define<ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext>(Name)
            .StartingAt(ChapterMembershipState.NotJoined)
            /* Write step only, and no commit: an import is a batch, and the caller commits the whole file at
               once. There being no Invite edge out of any other state is what stops a member who is already
               invited, or already in the group, being invited again. */
            .Transition(
                ChapterMembershipState.NotJoined,
                ChapterMembershipTrigger.Invite,
                ChapterMembershipState.Invited,
                x => x.Then<RaiseInvitation>())

            /* Drunken Knitwits only: signing up to a group there is joining it, and the membership is written
               alongside the account, before it can sign in. */
            .Transition(
                ChapterMembershipState.NotJoined,
                ChapterMembershipTrigger.SignUp,
                ChapterMembershipState.PendingApproval,
                x => signUp(x).When(approvalIsRequired))
            .Transition(
                ChapterMembershipState.NotJoined,
                ChapterMembershipTrigger.SignUp,
                ChapterMembershipState.Joined,
                x => signUp(x).When(Guard.Not(approvalIsRequired)))
            .Transition(
                ChapterMembershipState.Invited,
                ChapterMembershipTrigger.SignUp,
                ChapterMembershipState.Joined,
                x => signUp(x))

            .Transition(
                ChapterMembershipState.NotJoined,
                ChapterMembershipTrigger.Join,
                ChapterMembershipState.PendingApproval,
                x => join(x.When(approvalIsRequired)))
            .Transition(
                ChapterMembershipState.NotJoined,
                ChapterMembershipTrigger.Join,
                ChapterMembershipState.Joined,
                x => join(x.When(Guard.Not(approvalIsRequired))))

            /* An invitation is approval, so an invited member joining is never queued whatever the group's
               setting says - which is why this edge carries no guard. */
            .Transition(
                ChapterMembershipState.Invited,
                ChapterMembershipTrigger.Join,
                ChapterMembershipState.Joined,
                x => join(x))

            .Transition(
                ChapterMembershipState.PendingApproval,
                ChapterMembershipTrigger.Approve,
                ChapterMembershipState.Joined)
            .Build();
    }
}

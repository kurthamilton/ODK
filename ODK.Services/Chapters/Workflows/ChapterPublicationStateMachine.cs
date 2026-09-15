using ODK.Core.Workflows;
using ODK.Services.Chapters.Workflows.Guards;
using ODK.Services.Chapters.Workflows.Steps;
using ODK.Services.Workflows;

namespace ODK.Services.Chapters.Workflows;

/// <summary>
/// How a group becomes findable: its owner submits it, a site admin approves it, then its owner publishes
/// it. All three run through this machine.
/// </summary>
/// <remarks>
/// The state decides which move is legal, so approval carries no condition at all - the three dates on the
/// group say everything, and a group that has not submitted has no edge to approve along. The other two
/// moves have a guard each, both reading something that is not one of those dates: the checklist before
/// submission, and the picture before publication.
/// </remarks>
public static class ChapterPublicationStateMachine
{
    public const string Name = "Chapter publication";

    public static StateMachineDefinition<
        ChapterPublicationState, ChapterPublicationTrigger, ChapterPublicationContext> Create() => StateMachine
        .Define<ChapterPublicationState, ChapterPublicationTrigger, ChapterPublicationContext>(Name)
        .StartingAt(ChapterPublicationState.Draft)

        /* Only from Draft: a group is submitted once, and everything after it is further along. The guard is
           the checklist, which is what makes submission the owner saying they have finished rather than
           just a button. */
        .Transition(
            ChapterPublicationState.Draft,
            ChapterPublicationTrigger.Submit,
            ChapterPublicationState.Submitted,
            x => x
                .When(new ChecklistIsReadyToSubmit())
                .Then<MarkChapterSubmitted>()
                .Then<Commit<ChapterPublicationContext>>()
                .Then<SendGroupSubmittedEmail>())

        /* Submitting something already submitted changes nothing and is not a mistake, so these edges exist
           and do nothing rather than being absent and reporting the trigger as illegal. Everything past
           Submitted has been submitted by definition, which is why those have one too. */
        .Transition(
            ChapterPublicationState.Submitted,
            ChapterPublicationTrigger.Submit,
            ChapterPublicationState.Submitted)
        .Transition(
            ChapterPublicationState.Approved,
            ChapterPublicationTrigger.Submit,
            ChapterPublicationState.Approved)
        .Transition(
            ChapterPublicationState.Published,
            ChapterPublicationTrigger.Submit,
            ChapterPublicationState.Published)

        /* Only from Submitted: approving a group nobody has offered would take the decision about whether it
           is finished away from its owner, which is the whole of what submission is for. */
        .Transition(
            ChapterPublicationState.Submitted,
            ChapterPublicationTrigger.Approve,
            ChapterPublicationState.Approved,
            x => x
                .Then<MarkChapterApproved>()
                .Then<Commit<ChapterPublicationContext>>()
                .Then<SendGroupApprovedEmail>())

        /* Approving something already approved changes nothing and is not a mistake, so these edges exist and
           do nothing rather than being absent and reporting the trigger as illegal. A published group is
           approved by definition, which is why it has one too. */
        .Transition(
            ChapterPublicationState.Approved,
            ChapterPublicationTrigger.Approve,
            ChapterPublicationState.Approved)
        .Transition(
            ChapterPublicationState.Published,
            ChapterPublicationTrigger.Approve,
            ChapterPublicationState.Published)

        /* Only from Approved: a group that is not approved yet has nothing to publish, and a published one is
           already there. Both are the absence of an edge rather than a check. The picture is a guard instead, being a
           row on another table rather than something the state is derived from. */
        .Transition(
            ChapterPublicationState.Approved,
            ChapterPublicationTrigger.Publish,
            ChapterPublicationState.Published,
            x => x
                .When(new ImageIsPresent())
                .Then<MarkChapterPublished>()
                .Then<Commit<ChapterPublicationContext>>()
                .Then<SendInvitesWaitingEmail>())
        .Build();
}

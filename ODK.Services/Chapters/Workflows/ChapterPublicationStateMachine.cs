using ODK.Core.Workflows;
using ODK.Services.Chapters.Workflows.Steps;
using ODK.Services.Workflows;

namespace ODK.Services.Chapters.Workflows;

/// <summary>
/// How a group becomes findable: a site admin approves it, then its owner publishes it. Both run through this
/// machine.
/// </summary>
/// <remarks>
/// No guards. Which move is legal is decided entirely by the state, so there is nothing for a condition to
/// add - the two dates on the group say everything.
/// </remarks>
public static class ChapterPublicationStateMachine
{
    public const string Name = "Chapter publication";

    public static StateMachineDefinition<
        ChapterPublicationState, ChapterPublicationTrigger, ChapterPublicationContext> Create() => StateMachine
        .Define<ChapterPublicationState, ChapterPublicationTrigger, ChapterPublicationContext>(Name)
        .StartingAt(ChapterPublicationState.Draft)
        .Transition(
            ChapterPublicationState.Draft,
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

        /* Only from Approved: an unapproved group has nothing to publish yet, and a published one is already
           there. Both are the absence of an edge rather than a check. */
        .Transition(
            ChapterPublicationState.Approved,
            ChapterPublicationTrigger.Publish,
            ChapterPublicationState.Published,
            x => x
                .Then<MarkChapterPublished>()
                .Then<Commit<ChapterPublicationContext>>())
        .Build();
}

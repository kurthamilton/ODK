using ODK.Core.Workflows;

namespace ODK.Services.Chapters.Workflows;

/// <summary>
/// Derives how far a group has got from the three dates that record it. Nothing stores the state.
/// </summary>
public sealed class ChapterPublicationStateResolver
    : IStateResolver<ChapterPublicationState, ChapterPublicationContext>
{
    public ChapterPublicationState Resolve(ChapterPublicationContext context)
    {
        var chapter = context.Chapter;

        /* Approval is the outer gate: a group cannot be published before it is approved, so a publication date
           without an approval date is a group nobody can reach, and it reads as unapproved. That matches
           Chapter.IsOpenForRegistration, which also requires both.

           Read outwards from approval rather than inwards from submission, so a group approved before
           submission existed still reads as approved rather than being sent back to its owner. */
        if (!chapter.Approved())
        {
            return chapter.SubmittedForApproval()
                ? ChapterPublicationState.Submitted
                : ChapterPublicationState.Draft;
        }

        return chapter.IsPublished()
            ? ChapterPublicationState.Published
            : ChapterPublicationState.Approved;
    }
}

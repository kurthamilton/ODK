using ODK.Core.Workflows;

namespace ODK.Services.Chapters.Workflows;

/// <summary>
/// Derives how far a group has got from the two dates that record it. Nothing stores the state.
/// </summary>
public sealed class ChapterPublicationStateResolver
    : IStateResolver<ChapterPublicationState, ChapterPublicationContext>
{
    public ChapterPublicationState Resolve(ChapterPublicationContext context)
    {
        var chapter = context.Chapter;

        /* Approval is the outer gate: a group cannot be published before it is approved, so a publication date
           without an approval date is a group nobody can reach, and it reads as a draft. That matches
           Chapter.IsOpenForRegistration, which also requires both. */
        if (!chapter.Approved())
        {
            return ChapterPublicationState.Draft;
        }

        return chapter.IsPublished()
            ? ChapterPublicationState.Published
            : ChapterPublicationState.Approved;
    }
}

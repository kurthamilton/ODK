using ODK.Core.Workflows;

namespace ODK.Services.Chapters.Workflows.Guards;

/// <summary>
/// Whether everything the checklist puts before submission is behind the group. The rule is the
/// checklist's order rather than a list kept here: a step above the submission step is one the owner is
/// meant to have dealt with first, and an optional one counts as dealt with when it is skipped.
/// </summary>
public sealed class ChecklistIsReadyToSubmit : IGuard<ChapterPublicationContext>
{
    public string Description => "with the steps before it finished or skipped";

    public bool IsSatisfied(ChapterPublicationContext context) => context.RequiredChecklistReadyToSubmit;
}

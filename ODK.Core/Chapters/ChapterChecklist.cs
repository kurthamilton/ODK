namespace ODK.Core.Chapters;

/// <summary>
/// Resolves a group's checklist: the site's blueprint, what the group has already recorded against it, and
/// what the group's own state says has happened since.
/// </summary>
public static class ChapterChecklist
{
    /// <summary>
    /// Works out where a group stands on every step the blueprint defines, in its order.
    /// </summary>
    /// <remarks>
    /// A recorded row wins outright. Where there is none, the step is complete if the group's state says
    /// so, and completion is dated by the step's own timestamp wherever it has one - a group's creation,
    /// its approval, its publication, its first event. The steps evidenced only by the presence of
    /// something - a picture, a description, a topic - have no timestamp of their own, so
    /// <paramref name="utcNow"/> stands in: the earliest moment the group is known to have had it.
    /// </remarks>
    public static ChapterChecklistResolution Resolve(
        Chapter chapter,
        IReadOnlyCollection<ChecklistItem> items,
        IReadOnlyCollection<ChapterChecklistItem> recorded,
        ChapterChecklistFacts facts,
        DateTime utcNow)
    {
        var recordedByType = recorded.ToDictionary(x => x.ChecklistItemType);

        var states = new List<ChecklistItemState>();
        var unrecorded = new List<ChapterChecklistItem>();

        foreach (var item in items.OrderBy(x => x.DisplayOrder))
        {
            if (recordedByType.TryGetValue(item.Type, out var row))
            {
                states.Add(new ChecklistItemState
                {
                    CompletedUtc = row.CompletedUtc,
                    Dismissable = item.Dismissable,
                    DismissedUtc = row.DismissedUtc,
                    Type = item.Type
                });
                continue;
            }

            var completedUtc = GetCompletedUtc(item.Type, chapter, facts, utcNow);

            states.Add(new ChecklistItemState
            {
                CompletedUtc = completedUtc,
                Dismissable = item.Dismissable,
                DismissedUtc = null,
                Type = item.Type
            });

            if (completedUtc != null)
            {
                unrecorded.Add(new ChapterChecklistItem
                {
                    ChapterId = chapter.Id,
                    ChecklistItemType = item.Type,
                    CompletedUtc = completedUtc
                });
            }
        }

        return new ChapterChecklistResolution
        {
            Items = states,
            Unrecorded = unrecorded
        };
    }

    private static DateTime? GetCompletedUtc(
        ChecklistItemType type,
        Chapter chapter,
        ChapterChecklistFacts facts,
        DateTime utcNow)
        => type switch
        {
            ChecklistItemType.CreateGroup => chapter.CreatedUtc,
            ChecklistItemType.Picture => facts.HasImage ? utcNow : null,
            ChecklistItemType.Description =>
                facts.HasShortDescription && facts.HasDescription ? utcNow : null,
            ChecklistItemType.Questions => facts.HasQuestions ? utcNow : null,
            ChecklistItemType.MemberProperties => facts.HasMemberProperties ? utcNow : null,
            ChecklistItemType.Topics => facts.HasTopics ? utcNow : null,
            ChecklistItemType.SubmitForApproval => chapter.ApprovedUtc,
            ChecklistItemType.Publish => chapter.PublishedUtc,
            ChecklistItemType.FirstEvent => facts.FirstEventCreatedUtc,

            /* Membership and privacy settings, and anything a later blueprint row adds before the code
               that knows how to read it. A step nothing here can answer is complete only once a row says
               an organiser dealt with it - which is the safe answer either way, since it leaves the step
               on the checklist rather than quietly ticking it. */
            _ => null
        };
}

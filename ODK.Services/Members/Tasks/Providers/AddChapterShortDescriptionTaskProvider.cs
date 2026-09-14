using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Members.Tasks.Providers;

/// <summary>
/// Prompts an owner to summarise a group that has no short description - the line it is listed by, so
/// without one a group appears in the results saying nothing about itself.
///
/// Only where <see cref="ChapterTexts.ShowsShortDescription"/>: a platform that does not list groups
/// would be asking for a line it then never shows.
/// </summary>
public class AddChapterShortDescriptionTaskProvider : IMemberTaskProvider
{
    public IReadOnlyCollection<MemberTask> GetTasks(MemberTaskContext context)
    {
        if (!ChapterTexts.ShowsShortDescription(context.Platform))
        {
            return [];
        }

        return context.OwnedChapters
            .Where(x => !context.ChaptersWithShortDescription.Contains(x.Id))
            .Select(x => new MemberTask
            {
                Type = MemberTaskType.AddChapterShortDescription,
                Chapter = x
            })
            .ToArray();
    }
}

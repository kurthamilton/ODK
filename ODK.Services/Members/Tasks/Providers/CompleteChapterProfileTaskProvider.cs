using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Members.Tasks.Providers;

public class CompleteChapterProfileTaskProvider : IMemberTaskProvider
{
    public IReadOnlyCollection<MemberTask> GetTasks(MemberTaskContext context)
    {
        var valuesByPropertyId = context.MemberProperties
            .GroupBy(x => x.ChapterPropertyId)
            .ToDictionary(x => x.Key, x => x.First().Value);

        var propertiesByChapterId = context.ChapterProperties
            .GroupBy(x => x.ChapterId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        var tasks = new List<MemberTask>();
        foreach (var chapter in context.Chapters)
        {
            if (!propertiesByChapterId.TryGetValue(chapter.Id, out var chapterProperties))
            {
                continue;
            }

            var missing = chapterProperties.GetMissingRequired(valuesByPropertyId, forApplication: false);
            if (missing.Count > 0)
            {
                tasks.Add(new MemberTask
                {
                    Type = MemberTaskType.CompleteChapterProfile,
                    Chapter = chapter
                });
            }
        }

        return tasks;
    }
}

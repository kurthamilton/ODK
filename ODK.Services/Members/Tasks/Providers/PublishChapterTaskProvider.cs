using ODK.Core.Members;

namespace ODK.Services.Members.Tasks.Providers;

/// <summary>
/// Prompts an owner to publish a group that is approved but still unpublished - the last step before
/// anyone else can find it.
/// </summary>
public class PublishChapterTaskProvider : IMemberTaskProvider
{
    public IReadOnlyCollection<MemberTask> GetTasks(MemberTaskContext context) => context.OwnedChapters
        .Where(x => x.CanBePublished())
        .Select(x => new MemberTask
        {
            Type = MemberTaskType.PublishChapter,
            Chapter = x
        })
        .ToArray();
}

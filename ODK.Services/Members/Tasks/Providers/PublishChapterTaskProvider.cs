using ODK.Core.Members;

namespace ODK.Services.Members.Tasks.Providers;

/// <summary>
/// Prompts an owner to publish a group that is approved but still unpublished - the last step before
/// anyone else can find it. A group still missing the picture publication requires is not ready, and
/// <see cref="UploadChapterImageTaskProvider"/> asks for that instead.
/// </summary>
public class PublishChapterTaskProvider : IMemberTaskProvider
{
    public IReadOnlyCollection<MemberTask> GetTasks(MemberTaskContext context) => context.OwnedChapters
        .Where(x => x.CanBePublished(context.ChaptersWithImage.Contains(x.Id)))
        .Select(x => new MemberTask
        {
            Type = MemberTaskType.PublishChapter,
            Chapter = x
        })
        .ToArray();
}

using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Members.Tasks.Providers;

/// <summary>
/// Prompts an owner to add a picture to a group that has none.
///
/// Group Squirrel only: Drunken Knitwits never displays a group image, so there the picture would be
/// asked for and then never shown.
/// </summary>
public class UploadChapterImageTaskProvider : IMemberTaskProvider
{
    public IReadOnlyCollection<MemberTask> GetTasks(MemberTaskContext context)
    {
        if (context.Platform != PlatformType.Default)
        {
            return [];
        }

        return context.OwnedChapters
            .Where(x => !context.ChaptersWithImage.Contains(x.Id))
            .Select(x => new MemberTask
            {
                Type = MemberTaskType.UploadChapterImage,
                Chapter = x
            })
            .ToArray();
    }
}

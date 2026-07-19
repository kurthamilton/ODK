using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Members.Tasks.Providers;

public class UploadImageTaskProvider : IMemberTaskProvider
{
    public IReadOnlyCollection<MemberTask> GetTasks(MemberTaskContext context)
        => context.HasAvatar
            ? []
            : context.Platform == PlatformType.DrunkenKnitwits
                // DrunkKnitwits image is managed via the chapter profile, so choose their first chapter
                // (they should generally just have 1)
                ? context.Chapters.Count > 0
                    ? [new MemberTask { Type = MemberTaskType.UploadImage, Chapter = context.Chapters.First() }]
                    : []
                : [new MemberTask { Type = MemberTaskType.UploadImage }];
}

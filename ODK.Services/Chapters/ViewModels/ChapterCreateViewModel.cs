using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterCreateViewModel
{
    public required int ChapterCount { get; init; }

    public required int? ChapterLimit { get; init; }

    public required Member Member { get; init; }

    public required MemberLocation MemberLocation { get; init; }

    public required PlatformType Platform { get; init; }
}

using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterMessageAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterContactMessage Message { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<ChapterContactMessageReply> Replies { get; init; }
}

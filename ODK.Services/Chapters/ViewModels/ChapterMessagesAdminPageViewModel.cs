using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterMessagesAdminPageViewModel
{
    public required Chapter Chapter { get; init; } 

    public required bool IsSpam { get; init; }

    public required int MessageCount { get; init; }

    public required IReadOnlyCollection<ChapterContactMessage> Messages { get; init; }

    public required PlatformType Platform { get; init; }

    public required int SpamMessageCount { get; init; }
}

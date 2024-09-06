using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterConversationsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterConversationDto> Conversations { get; init; }

    public required PlatformType Platform { get; init; }    

    public required ChapterPrivacySettings? PrivacySettings { get; init; }

    public required bool ReadByChapter { get; init; }
}

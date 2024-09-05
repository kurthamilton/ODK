using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Contact.ViewModels;

public class ChapterContactPageViewModel
{
    public required bool CanStartConversation { get; init; }

    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterConversation> Conversations { get; init; }

    public required PlatformType Platform { get; init; }
}

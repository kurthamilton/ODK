using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterConversationsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterConversationDto> Conversations { get; init; }

    public required ChapterConversationStatus Status { get; init; }
}
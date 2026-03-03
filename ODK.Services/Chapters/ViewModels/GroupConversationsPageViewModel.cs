using ODK.Data.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class GroupConversationsPageViewModel : GroupPageViewModel
{
    public required int ActiveConversationCount { get; init; }

    public required bool Archived { get; init; }

    public required int ArchivedConversationCount { get; init; }

    public required IReadOnlyCollection<ChapterConversationDto> Conversations { get; init; }
}
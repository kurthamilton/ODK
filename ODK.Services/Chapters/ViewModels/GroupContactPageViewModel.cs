using ODK.Data.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class GroupContactPageViewModel : GroupPageViewModel
{
    public required bool CanStartConversation { get; init; }

    public required IReadOnlyCollection<ChapterConversationDto> Conversations { get; init; }
}
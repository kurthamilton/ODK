using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class GroupConversationPageViewModel : GroupPageViewModel
{
    public required ChapterConversation Conversation { get; init; }

    public required IReadOnlyCollection<ChapterConversationMessage> Messages { get; init; }

    public required IReadOnlyCollection<ChapterConversationDto> OtherConversations { get; init; }
}

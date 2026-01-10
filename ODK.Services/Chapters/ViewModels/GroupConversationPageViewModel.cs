using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class GroupConversationPageViewModel : GroupPageViewModel
{
    public required ChapterConversation Conversation { get; init; }

    public required IReadOnlyCollection<ChapterConversationMessage> Messages { get; init; }

    public required IReadOnlyCollection<ChapterConversationDto> OtherConversations { get; init; }
}

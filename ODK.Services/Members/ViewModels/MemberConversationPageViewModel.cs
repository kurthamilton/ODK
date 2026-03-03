using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;

namespace ODK.Services.Members.ViewModels;

public class MemberConversationPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterConversation Conversation { get; init; }

    public required IReadOnlyCollection<ChapterConversationMessageDto> Messages { get; init; }

    public required IReadOnlyCollection<ChapterConversationDto> OtherConversations { get; init; }
}
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterConversationAdminPageViewModel
{
    public required bool CanReply { get; init; }

    public required Chapter Chapter { get; init; }

    public required ChapterConversation Conversation { get; init; }

    public required Member CurrentMember { get; init; }

    public required Member Member { get; init; }

    public required IReadOnlyCollection<ChapterConversationMessageDto> Messages { get; init; }

    public required IReadOnlyCollection<ChapterConversationDto> OtherConversations { get; init; }
}

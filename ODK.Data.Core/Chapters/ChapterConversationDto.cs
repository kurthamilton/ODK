using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Data.Core.Chapters;

public class ChapterConversationDto
{
    public required ChapterConversation Conversation { get; init; }

    public required ChapterConversationMessageDto LastMessage { get; init; }

    public required Member Member { get; init; }
}
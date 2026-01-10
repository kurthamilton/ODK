using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Data.Core.Chapters;

public class ChapterConversationDto
{
    public required ChapterConversation Conversation { get; init; }

    public required ChapterConversationMessage LastMessage { get; init; }

    public required Member Member { get; init; }

    public required MemberSubscription? MemberSubscription { get; init; }
}
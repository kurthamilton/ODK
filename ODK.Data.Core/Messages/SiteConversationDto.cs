using ODK.Core.Members;
using ODK.Core.Messages;

namespace ODK.Data.Core.Messages;

public class SiteConversationDto
{
    public required SiteConversation Conversation { get; init; }

    public required SiteConversationMessageDto LastMessage { get; init; }

    public required Member Member { get; init; }

    public required int MessageCount { get; init; }
}

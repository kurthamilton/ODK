using ODK.Core.Messages;
using ODK.Data.Core.Messages;

namespace ODK.Services.Members.ViewModels;

public class SiteConversationPageViewModel
{
    public required SiteConversation Conversation { get; init; }

    public required IReadOnlyCollection<SiteConversationMessageDto> Messages { get; init; }

    /// <summary>The member's other site conversations, for the sidebar list.</summary>
    public required IReadOnlyCollection<SiteConversationDto> OtherConversations { get; init; }
}

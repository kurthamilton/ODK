using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Data.Core.Messages;

namespace ODK.Services.Contact.ViewModels;

public class SiteConversationAdminPageViewModel
{
    public required SiteConversation Conversation { get; init; }

    /// <summary>The member whose thread it is, for the admin to see who they are answering.</summary>
    public required Member Member { get; init; }

    public required IReadOnlyCollection<SiteConversationMessageDto> Messages { get; init; }
}

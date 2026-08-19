using ODK.Data.Core.Messages;

namespace ODK.Services.Contact.ViewModels;

public class SiteConversationsAdminPageViewModel
{
    public required int ActiveConversationCount { get; init; }

    public required bool Archived { get; init; }

    public required int ArchivedConversationCount { get; init; }

    /// <summary>Most recently active first: a thread is interesting because somebody just said something.</summary>
    public required IReadOnlyCollection<SiteConversationDto> Conversations { get; init; }
}

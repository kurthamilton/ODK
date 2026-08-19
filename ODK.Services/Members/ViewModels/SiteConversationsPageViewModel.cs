using ODK.Core.Members;
using ODK.Data.Core.Messages;

namespace ODK.Services.Members.ViewModels;

public class SiteConversationsPageViewModel
{
    public required int ActiveConversationCount { get; init; }

    public required bool Archived { get; init; }

    public required int ArchivedConversationCount { get; init; }

    public required IReadOnlyCollection<SiteConversationDto> Conversations { get; init; }

    public required Member CurrentMember { get; init; }

    public int TotalConversationCount => ActiveConversationCount + ArchivedConversationCount;
}

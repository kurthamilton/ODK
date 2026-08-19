using ODK.Core.Members;
using ODK.Data.Core.Messages;

namespace ODK.Web.Razor.Models.Conversations;

/// <summary>
/// A member's site conversations. Its own model rather than the group list's: that one carries a Group
/// column for the across-all-groups view and starts threads through a form requiring a reCAPTCHA token,
/// neither of which the site has. Merging them was adding more weight than the duplication saves.
/// </summary>
public class SiteConversationsViewModel
{
    public required int ActiveConversationCount { get; init; }

    public required bool Archived { get; init; }

    public required int ArchivedConversationCount { get; init; }

    public required IReadOnlyCollection<SiteConversationDto> Conversations { get; init; }

    public required Func<bool, string> ConversationsUrlFunc { get; init; }

    public required Func<Guid, string> ConversationUrlFunc { get; init; }

    public required Member CurrentMember { get; init; }

    public TimeZoneInfo TimeZone => CurrentMember.TimeZone;

    public int TotalConversationCount => ActiveConversationCount + ArchivedConversationCount;
}

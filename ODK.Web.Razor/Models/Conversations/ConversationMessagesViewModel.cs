using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Conversations;

/// <summary>
/// Just the thread. Its own model because the group admin screen renders the messages without the reply box,
/// the breadcrumbs or the other-conversations list, and asking it for a whole
/// <see cref="ConversationViewModel"/> meant handing over URLs and a subject nothing was going to read.
/// </summary>
public class ConversationMessagesViewModel
{
    public required Member CurrentMember { get; init; }

    public required IReadOnlyCollection<ConversationMessageViewModel> Messages { get; init; }

    public TimeZoneInfo TimeZone => CurrentMember.TimeZone;
}

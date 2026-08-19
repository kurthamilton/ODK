using ODK.Core.Members;
using ODK.Web.Razor.Models.Components;

namespace ODK.Web.Razor.Models.Conversations;

/// <summary>
/// One conversation and its thread, for the page that renders it.
/// </summary>
/// <remarks>
/// Deliberately says nothing about where the conversation lives. A member's thread with a group and a
/// member's thread with the site are the same screen - a subject, some messages, a reply box and a list of
/// their other threads - so this carries that shape rather than either side's entities, and the pages map
/// into it. The endpoints differ between the two, which is why the form actions arrive as URLs rather than
/// being written into the partial.
/// </remarks>
public class ConversationViewModel
{
    public required DateTime? ArchivedUtc { get; init; }

    /// <summary>Where the archive form posts.</summary>
    public required string ArchiveUrl { get; init; }

    public required IReadOnlyCollection<MenuItem> Breadcrumbs { get; init; }

    public required Guid ConversationId { get; init; }

    /// <summary>Builds the link to another of the member's conversations.</summary>
    public required Func<Guid, string> ConversationUrlFunc { get; init; }

    public required Member CurrentMember { get; init; }

    public required IReadOnlyCollection<ConversationMessageViewModel> Messages { get; init; }

    public required IReadOnlyCollection<ConversationSummaryViewModel> OtherConversations { get; init; }

    /// <summary>Where the reply form posts.</summary>
    public required string ReplyUrl { get; init; }

    /// <summary>Where the restore form posts, for a conversation that has been archived.</summary>
    public required string RestoreUrl { get; init; }

    public required string Subject { get; init; }

    public TimeZoneInfo TimeZone => CurrentMember.TimeZone;
}

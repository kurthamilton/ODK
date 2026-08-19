namespace ODK.Web.Razor.Models.Conversations;

/// <summary>A conversation as the "other conversations" list shows it: enough to name it and link to it.</summary>
public class ConversationSummaryViewModel
{
    public required Guid Id { get; init; }

    public required DateTime LastMessageUtc { get; init; }

    public required int MessageCount { get; init; }

    public required string Subject { get; init; }
}

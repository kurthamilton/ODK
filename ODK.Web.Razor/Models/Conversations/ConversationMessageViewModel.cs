namespace ODK.Web.Razor.Models.Conversations;

/// <summary>One message in a conversation, as the thread renders it.</summary>
public class ConversationMessageViewModel
{
    public required DateTime CreatedUtc { get; init; }

    public required Guid MemberId { get; init; }

    /// <summary>
    /// The sender as this reader is shown them, which is not always their own name: a site admin's reply
    /// reads as the site to the member it answers.
    /// </summary>
    public required string MemberFullName { get; init; }

    public required string Text { get; init; }
}

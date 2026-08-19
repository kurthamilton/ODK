using ODK.Data.Core.Chapters;
using ODK.Web.Razor.Models.Conversations;

namespace ODK.Web.Razor.Models.Chapters;

/// <summary>
/// Maps a group's conversations onto the shape the conversation screens render. Here rather than beside those
/// view models so that they stay free of chapter types - the site's conversations map onto the same shape
/// from their own.
/// </summary>
public static class ChapterConversationMappings
{
    public static IReadOnlyCollection<ConversationMessageViewModel> ToViewModels(
        this IEnumerable<ChapterConversationMessageDto> messages) => messages
        .Select(x => new ConversationMessageViewModel
        {
            CreatedUtc = x.Message.CreatedUtc,
            MemberFullName = x.MemberFullName,
            MemberId = x.Message.MemberId,
            Text = x.Message.Text
        })
        .ToArray();

    public static IReadOnlyCollection<ConversationSummaryViewModel> ToViewModels(
        this IEnumerable<ChapterConversationDto> conversations) => conversations
        .Select(x => new ConversationSummaryViewModel
        {
            Id = x.Conversation.Id,
            LastMessageUtc = x.LastMessage.Message.CreatedUtc,
            MessageCount = x.MessageCount,
            Subject = x.Conversation.Subject
        })
        .ToArray();
}

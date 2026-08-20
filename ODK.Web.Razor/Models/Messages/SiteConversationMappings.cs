using ODK.Core.Messages;
using ODK.Data.Core.Messages;
using ODK.Web.Razor.Models.Conversations;

namespace ODK.Web.Razor.Models.Messages;

/// <summary>
/// Maps the site's conversations onto the shape the conversation screens render, as
/// <c>ChapterConversationMappings</c> does for a group's.
/// </summary>
/// <remarks>
/// The thread has two mappings because the two sides of it are shown different things: a member sees the
/// site, an admin sees their colleagues.
/// </remarks>
public static class SiteConversationMappings
{
    /// <summary>
    /// The thread as the member whose conversation it is reads it. Anything they did not write came from a
    /// site admin and is attributed to the platform rather than to the person: the member is talking to the
    /// site, and which admin picked their conversation up is not theirs to know.
    /// </summary>
    /// <remarks>
    /// Who the site side is follows from the conversation's own member rather than from asking whether the
    /// sender is an admin, so a reply keeps reading as the site's whatever becomes of the account that wrote
    /// it. <see cref="ConversationMessageViewModel.MemberId"/> stays the real one - the thread aligns each
    /// message and labels the member's own as "You" from it, and nothing renders it.
    /// </remarks>
    public static IReadOnlyCollection<ConversationMessageViewModel> ToMemberViewModels(
        this IEnumerable<SiteConversationMessageDto> messages,
        SiteConversation conversation,
        string platformName) => messages
        .Select(x => new ConversationMessageViewModel
        {
            CreatedUtc = x.Message.CreatedUtc,
            MemberFullName = x.Message.MemberId == conversation.MemberId
                ? x.MemberFullName
                : platformName,
            MemberId = x.Message.MemberId,
            Text = x.Message.Text
        })
        .ToArray();

    /// <summary>
    /// The thread as the site admin area shows it, with every sender named. Which admin answered a
    /// conversation is worth seeing from inside the admin area, where the audience is the admins themselves.
    /// </summary>
    public static IReadOnlyCollection<ConversationMessageViewModel> ToSiteAdminViewModels(
        this IEnumerable<SiteConversationMessageDto> messages) => messages
        .Select(x => new ConversationMessageViewModel
        {
            CreatedUtc = x.Message.CreatedUtc,
            MemberFullName = x.MemberFullName,
            MemberId = x.Message.MemberId,
            Text = x.Message.Text
        })
        .ToArray();

    public static IReadOnlyCollection<ConversationSummaryViewModel> ToViewModels(
        this IEnumerable<SiteConversationDto> conversations) => conversations
        .Select(x => new ConversationSummaryViewModel
        {
            Id = x.Conversation.Id,
            LastMessageUtc = x.LastMessage.Message.CreatedUtc,
            MessageCount = x.MessageCount,
            Subject = x.Conversation.Subject
        })
        .ToArray();
}

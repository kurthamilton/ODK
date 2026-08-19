    namespace ODK.Core.Messages;

/// <summary>
/// One message in a <see cref="SiteConversation"/>, from the member or from a site admin.
/// </summary>
/// <remarks>
/// <para>
/// Carries no reCAPTCHA score, unlike its chapter counterpart: only a signed-in member can start one of
/// these, and what protects authed contact is a story of its own. A column nothing sets would be a promise
/// the code does not keep.
/// </para>
/// <para>
/// Read state is <em>when</em> rather than <em>whether</em>. The chapter equivalent stores a bool, which
/// answers the only question asked of it today and throws away the rest - how long a member waited to be
/// answered, or an admin to notice, is not recoverable once it has been reduced to true.
/// </para>
/// </remarks>
public class SiteConversationMessage : IDatabaseEntity
{
    public DateTime CreatedUtc { get; set; }

    /// <summary>When the member first saw it. Null until they do; set on the messages an admin sends.</summary>
    public DateTime? FirstReadByMemberUtc { get; set; }

    /// <summary>
    /// When the site side first saw it. One admin reading marks it read for all of them, which is what the
    /// chapter side does with a group's admins.
    /// </summary>
    public DateTime? FirstReadBySiteAdminUtc { get; set; }

    public Guid Id { get; set; }

    /// <summary>Who sent it - the conversation's member, or the site admin replying.</summary>
    public Guid MemberId { get; set; }

    public Guid SiteConversationId { get; set; }

    public required string Text { get; set; }
}

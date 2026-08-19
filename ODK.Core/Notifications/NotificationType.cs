namespace ODK.Core.Notifications;

public enum NotificationType
{
    None = 0,
    NewMember = 1,
    NewEvent = 2,
    ChapterContactMessage = 3,
    ConversationOwnerMessage = 4,
    ConversationReplies = 5,
    EventWaitlistPromotion = 6,

    /// <summary>A member has written to the site's admins.</summary>
    SiteConversationMemberMessage = 7,

    /// <summary>A site admin has replied to the member's thread.</summary>
    SiteConversationReplies = 8
}

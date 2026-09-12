using System.ComponentModel.DataAnnotations;

namespace ODK.Core.Emails;

public enum EmailType
{
    None = 0,

    [Display(Name = "Password reset")]
    PasswordReset = 1,

    [Display(Name = "Activate account")]
    ActivateAccount = 2,

    [Display(Name = "Event invite")]
    EventInvite = 3,

    [Display(Name = "Contact message (admin)")]
    ContactRequest = 4,

    [Display(Name = "New member")]
    NewMember = 5,

    [Display(Name = "New member (admin)")]
    NewMemberAdmin = 6,

    [Display(Name = "Email address change")]
    EmailAddressUpdate = 7,

    [Display(Name = "Layout")]
    Layout = 8,

    [Display(Name = "Membership payment confirmation")]
    SubscriptionConfirmation = 9,

    [Display(Name = "Event comment (admin)")]
    EventComment = 10,

    [Display(Name = "Sign up with an existing account")]
    DuplicateEmail = 11,

    [Display(Name = "Membership expiring")]
    SubscriptionExpiring = 12,

    [Display(Name = "Membership expired")]
    SubscriptionExpired = 13,

    [Display(Name = "Trial expiring")]
    TrialExpiring = 14,

    [Display(Name = "Trial expired")]
    TrialExpired = 15,

    [Display(Name = "Imported member: activate account")]
    MemberImportActivation = 16,

    [Display(Name = "Imported member: invited to group")]
    MemberImportInvite = 17,

    [Display(Name = "Event comment reply")]
    EventCommentReply = 18,

    [Display(Name = "Payment notification")]
    PaymentNotification = 19,

    [Display(Name = "Group approved")]
    GroupApproved = 20,

    [Display(Name = "Membership approved")]
    MemberApproved = 21,

    [Display(Name = "Site welcome")]
    SiteWelcome = 22,

    [Display(Name = "Site subscription expired")]
    SiteSubscriptionExpired = 23,

    [Display(Name = "New group (admin)")]
    NewGroupAdmin = 24,

    [Display(Name = "Event waitlist place")]
    EventWaitlistPromotion = 25,

    [Display(Name = "Member left (admin)")]
    MemberLeftAdmin = 26,

    [Display(Name = "Contact message reply")]
    ContactRequestReply = 27,

    [Display(Name = "Site contact message reply")]
    SiteContactRequestReply = 28,

    [Display(Name = "New topics (admin)")]
    NewTopicAdmin = 29,

    [Display(Name = "Topics approved")]
    TopicsApproved = 30,

    [Display(Name = "Topics rejected")]
    TopicsRejected = 31,

    [Display(Name = "Conversation message")]
    ConversationMessage = 32,

    [Display(Name = "Conversation message (admin)")]
    ConversationMessageAdmin = 33,

    [Display(Name = "Site conversation message")]
    SiteConversationMessage = 34,

    [Display(Name = "Site conversation message (admin)")]
    SiteConversationMessageAdmin = 35,

    [Display(Name = "Membership removed")]
    MemberRemoved = 36,

    [Display(Name = "Invites waiting to be sent")]
    InvitesWaiting = 37
}

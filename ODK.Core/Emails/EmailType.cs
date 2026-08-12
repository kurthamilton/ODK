using System.ComponentModel.DataAnnotations;

namespace ODK.Core.Emails;

public enum EmailType
{
    None = 0,

    [Display(Name = "Password reset")]
    PasswordReset = 1,

    [Display(Name = "Activate account")]
    ActivateAccount = 2,

    [Display(Name = "Event invitation")]
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

    [Display(Name = "Imported member: added to group")]
    MemberImportInvite = 17,

    [Display(Name = "Event comment reply")]
    EventCommentReply = 18
}

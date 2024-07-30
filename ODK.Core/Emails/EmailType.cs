namespace ODK.Core.Emails;

public enum EmailType
{
    None = 0,
    PasswordReset = 1,
    ActivateAccount = 2,
    EventInvite = 3,
    ContactRequest = 4,
    NewMember = 5,
    NewMemberAdmin = 6,
    EmailAddressUpdate = 7,
    Layout = 8,
    SubscriptionConfirmation = 9,
    EventComment = 10
}

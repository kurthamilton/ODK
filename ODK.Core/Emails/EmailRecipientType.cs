namespace ODK.Core.Emails;

/// <summary>
/// Who an email is written for. Values are explicit because they are stored in
/// <c>Emails.EmailRecipientTypeId</c> and mirrored in the <c>EmailRecipientTypes</c> lookup table, so the
/// number is the contract.
/// </summary>
public enum EmailRecipientType
{
    None = 0,
    Admins = 1,
    Members = 2
}

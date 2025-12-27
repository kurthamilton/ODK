namespace ODK.Core.Members;

public class MemberEmailAddressUpdateToken
{
    public string ConfirmationToken { get; set; } = string.Empty;

    public Guid MemberId { get; set; }

    public string NewEmailAddress { get; set; } = string.Empty;
}

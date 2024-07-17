namespace ODK.Core.Members;

public class MemberEmailAddressUpdateToken
{
    public string ConfirmationToken { get; set; } = "";

    public Guid MemberId { get; set; }

    public string NewEmailAddress { get; set; } = "";
}

namespace ODK.Core.Members;

public class MemberEmailAddressUpdateToken
{
    public MemberEmailAddressUpdateToken(Guid memberId, string newEmailAddress, string confirmationToken)
    {
        ConfirmationToken = confirmationToken;
        MemberId = memberId;
        NewEmailAddress = newEmailAddress;
    }

    public string ConfirmationToken { get; }

    public Guid MemberId { get; }

    public string NewEmailAddress { get; }
}

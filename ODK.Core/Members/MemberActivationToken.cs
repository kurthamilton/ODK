namespace ODK.Core.Members;

public class MemberActivationToken
{
    public string ActivationToken { get; set; } = "";

    public Guid MemberId { get; set; }
}

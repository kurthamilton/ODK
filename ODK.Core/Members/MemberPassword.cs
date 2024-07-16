namespace ODK.Core.Members;

public class MemberPassword
{
    public Guid MemberId { get; set; }

    public string Password { get; set; } = "";

    public string Salt { get; set; } = "";
}

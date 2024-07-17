namespace ODK.Core.Members;

public class MemberPassword
{
    public string Hash { get; set; } = "";

    public Guid MemberId { get; set; }    

    public string Salt { get; set; } = "";
}

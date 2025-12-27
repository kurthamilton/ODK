namespace ODK.Core.Members;

public class MemberPassword
{
    public string Hash { get; set; } = string.Empty;

    public Guid MemberId { get; set; }    

    public string Salt { get; set; } = string.Empty;
}

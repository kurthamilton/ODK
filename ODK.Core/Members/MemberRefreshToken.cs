namespace ODK.Core.Members;

public class MemberRefreshToken
{
    public DateTime Expires { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public string RefreshToken { get; set; } = "";
}

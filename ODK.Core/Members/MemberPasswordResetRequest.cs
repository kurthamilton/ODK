namespace ODK.Core.Members;

public class MemberPasswordResetRequest : IDatabaseEntity
{
    public DateTime Created { get; set; }

    public DateTime Expires { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public string Token { get; set; } = "";
}

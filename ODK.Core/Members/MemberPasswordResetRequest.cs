namespace ODK.Core.Members;

public class MemberPasswordResetRequest : IDatabaseEntity
{
    public DateTime CreatedUtc { get; set; }

    public DateTime ExpiresUtc { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public string Token { get; set; } = string.Empty;
}

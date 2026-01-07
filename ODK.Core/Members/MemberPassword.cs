namespace ODK.Core.Members;

public class MemberPassword : IHashedPassword
{
    public string Algorithm { get; set; } = string.Empty;

    public string Hash { get; set; } = string.Empty;

    public int Iterations { get; set; }

    public Guid MemberId { get; set; }

    public string Salt { get; set; } = string.Empty;
}

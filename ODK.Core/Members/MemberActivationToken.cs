namespace ODK.Core.Members;

public class MemberActivationToken
{
    public string ActivationToken { get; set; } = string.Empty;

    public Guid? ChapterId { get; set; }

    public Guid MemberId { get; set; }
}

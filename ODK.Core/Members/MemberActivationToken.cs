namespace ODK.Core.Members;

public class MemberActivationToken
{
    public string ActivationToken { get; set; } = string.Empty;

    public Guid? ChapterId { get; set; }

    /// <summary>
    /// What the sign-up that issued this token was for. Null where it stated nothing, which is every
    /// sign-up that reached the site by its own front door.
    /// </summary>
    public SignUpIntentType? Intent { get; set; }

    public Guid MemberId { get; set; }
}

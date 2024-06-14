namespace ODK.Core.Members;

public class MemberPassword
{
    public MemberPassword(Guid memberId, string password, string salt)
    {
        MemberId = memberId;
        Password = password;
        Salt = salt;
    }

    public Guid MemberId { get; }

    public string Password { get; }

    public string Salt { get; }
}

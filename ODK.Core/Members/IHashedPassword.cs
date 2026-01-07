namespace ODK.Core.Members;

public interface IHashedPassword : IHashedPasswordOptions
{
    public string Hash { get; }
}

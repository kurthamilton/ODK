namespace ODK.Core.Members;

public interface IHashedPasswordOptions
{
    string Algorithm { get; }

    int Iterations { get; }

    string Salt { get; }
}

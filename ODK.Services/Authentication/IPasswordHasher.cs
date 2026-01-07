using ODK.Core.Members;

namespace ODK.Services.Authentication;

public interface IPasswordHasher
{
    bool Check(string plainText, IHashedPassword hashed);

    (string hash, IHashedPasswordOptions options) ComputeHash(string plainText);    

    bool ShouldUpdate(IHashedPassword hashed);
}

using System.Security.Cryptography;
using ODK.Core.Members;

namespace ODK.Services.Authentication;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltByteSize = 24;
    private const int HashByteSize = 24;

    private readonly PasswordHasherSettings _settings;

    public PasswordHasher(PasswordHasherSettings settings)
    {
        _settings = settings;
    }

    public bool Check(string plainText, IHashedPassword hashed)
    {
        var hashBytes = ComputeHashBytes(plainText, hashed);
        var hash = Convert.ToBase64String(hashBytes);
        return string.Equals(hash, hashed.Hash, StringComparison.Ordinal);
    }

    public (string hash, IHashedPasswordOptions options) ComputeHash(string plainText)
    {
        var salt = GenerateSaltBytes();
        var base64Salt = Convert.ToBase64String(salt);

        var options = GetRecommendedOptions(base64Salt);
        var passwordHash = ComputeHashBytes(plainText, options);

        var base64PasswordHash = Convert.ToBase64String(passwordHash);

        return (base64PasswordHash, options);
    }

    public bool ShouldUpdate(IHashedPassword hashed)
        => hashed.Algorithm != _settings.Algorithm || hashed.Iterations != _settings.Iterations;

    private byte[] ComputeHashBytes(string plainText, IHashedPasswordOptions options)
        => Rfc2898DeriveBytes.Pbkdf2(
            plainText,
            Convert.FromBase64String(options.Salt),
            options.Iterations,
            new HashAlgorithmName(options.Algorithm),
            HashByteSize);

    private byte[] GenerateSaltBytes() => RandomNumberGenerator.GetBytes(SaltByteSize);

    private IHashedPasswordOptions GetRecommendedOptions(string salt) => new PasswordHasherOptions
    {
        Algorithm = _settings.Algorithm,
        Iterations = _settings.Iterations,
        Salt = salt
    };
}

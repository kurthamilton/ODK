using System.Security.Cryptography;

namespace ODK.Services.Authentication;

public class PasswordHasher
{
    private const int SaltByteSize = 24;
    private const int HashByteSize = 24;
    private const int HasingIterationsCount = 10101;

    public static (string hash, string salt) ComputeHash(string plainText)
    {
        byte[] salt = GenerateSalt();
        byte[] passwordHash = ComputeHash(plainText, salt);

        string base64Salt = Convert.ToBase64String(salt);
        string base64PasswordHash = Convert.ToBase64String(passwordHash);

        return (base64PasswordHash, base64Salt);
    }

    public static string ComputeHash(string plainText, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        byte[] passwordHash = ComputeHash(plainText, saltBytes);

        return Convert.ToBase64String(passwordHash);
    }

    private static byte[] ComputeHash(string plainText, byte[] salt, int iterations = HasingIterationsCount, int hashByteSize = HashByteSize)
    {
        using var hashGenerator = new Rfc2898DeriveBytes(plainText, salt, iterations, HashAlgorithmName.SHA1);
        hashGenerator.IterationCount = iterations;
        return hashGenerator.GetBytes(hashByteSize);
    }

    private static byte[] GenerateSalt(int saltByteSize = SaltByteSize) => RandomNumberGenerator.GetBytes(saltByteSize);
}

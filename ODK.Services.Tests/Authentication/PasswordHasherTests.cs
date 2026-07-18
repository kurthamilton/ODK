using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Services.Authentication;

namespace ODK.Services.Tests.Authentication;

[Parallelizable]
public static class PasswordHasherTests
{
    [Test]
    public static void Check_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var hasher = CreateHasher();
        var (hash, options) = hasher.ComputeHash("correct horse battery staple");
        var stored = ToMemberPassword(hash, options);

        // Act
        var result = hasher.Check("correct horse battery staple", stored);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void Check_MalformedStoredHash_ReturnsFalse()
    {
        // Arrange
        var hasher = CreateHasher();
        var (_, options) = hasher.ComputeHash("correct horse battery staple");
        var stored = ToMemberPassword("not-valid-base64!!!", options);

        // Act
        var result = hasher.Check("correct horse battery staple", stored);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void Check_WrongPassword_ReturnsFalse()
    {
        // Arrange
        var hasher = CreateHasher();
        var (hash, options) = hasher.ComputeHash("correct horse battery staple");
        var stored = ToMemberPassword(hash, options);

        // Act
        var result = hasher.Check("wrong password", stored);

        // Assert
        result.Should().BeFalse();
    }

    private static PasswordHasher CreateHasher()
        => new PasswordHasher(new PasswordHasherSettings
        {
            Algorithm = "SHA256",
            Iterations = 10_000
        });

    private static MemberPassword ToMemberPassword(string hash, IHashedPasswordOptions options)
        => new MemberPassword
        {
            Algorithm = options.Algorithm,
            Hash = hash,
            Iterations = options.Iterations,
            Salt = options.Salt
        };
}

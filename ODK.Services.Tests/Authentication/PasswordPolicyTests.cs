using FluentAssertions;
using NUnit.Framework;
using ODK.Services.Authentication;

namespace ODK.Services.Tests.Authentication;

[Parallelizable]
public static class PasswordPolicyTests
{
    private const int Min = 8;

    [Test]
    public static void GetValidationError_AtMinLength_ReturnsNull()
    {
        CreatePolicy().GetValidationError(new string('a', Min))
            .Should().BeNull();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public static void GetValidationError_BlankPassword_ReturnsError(string? password)
    {
        CreatePolicy().GetValidationError(password)
            .Should().Be("Password cannot be blank");
    }

    [Test]
    public static void GetValidationError_TooShort_ReturnsError()
    {
        CreatePolicy().GetValidationError(new string('a', Min - 1))
            .Should().Be($"Password must be at least {Min} characters");
    }

    private static PasswordPolicy CreatePolicy()
        => new PasswordPolicy(new PasswordPolicySettings
        {
            MinLength = Min
        });
}

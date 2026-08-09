using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Services.Emails;
using ODK.Services.Emails.Validation;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Emails;

[Parallelizable]
public static class EmailValidationServiceTests
{
    // Addresses the previous pattern let through. Each is well-formed enough to look plausible but can
    // never be delivered to, so catching them here saves a wasted verification credit as well as a bounce.
    [TestCase(".leading.dot@example.com")]
    [TestCase("trailing.dot.@example.com")]
    [TestCase("double..dot@example.com")]
    [TestCase("user@example..com")]
    [TestCase("user@-example.com")]
    [TestCase("user@example-.com")]
    [TestCase("user@localhost")]
    public static async Task Validate_MalformedAddressThePreviousPatternAllowed_Fails(string emailAddress)
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.Validate(emailAddress, EmailValidationLevel.Full);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email address format");
    }

    [TestCase("user@example.com")]
    [TestCase("first.last@example.co.uk")]
    [TestCase("user+tag@example.com")]
    [TestCase("user_name@sub.example.com")]
    [TestCase("user-name@example-domain.com")]
    [TestCase("o'brien@example.com")]
    public static async Task Validate_RealWorldAddress_Passes(string emailAddress)
    {
        // Arrange - the tightened pattern must not start rejecting addresses people actually hold.
        var service = CreateService();

        // Act
        var result = await service.Validate(emailAddress, EmailValidationLevel.Soft);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task Validate_Soft_DoesNotCallTheVerifier()
    {
        // Arrange - anonymous public forms must never spend a verification credit, whatever they submit.
        var verifier = new Mock<IEmailVerifier>();
        verifier.Setup(x => x.Verify(It.IsAny<string>()))
            .ReturnsAsync(EmailVerificationResult.Invalid);
        var service = new EmailValidationService(verifier.Object);

        // Act
        var result = await service.Validate("user@example.com", EmailValidationLevel.Soft);

        // Assert - passes despite the verifier being set up to reject, because it is never asked.
        result.Success.Should().BeTrue();
        verifier.Verify(x => x.Verify(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public static async Task Validate_UnsetLevel_DoesNotCallTheVerifier()
    {
        // Arrange - None is the zero value, so it is what an uninitialised level would be. It must not
        // silently buy the most expensive behaviour.
        var verifier = new Mock<IEmailVerifier>();
        var service = new EmailValidationService(verifier.Object);

        // Act
        var result = await service.Validate("user@example.com", EmailValidationLevel.None);

        // Assert
        result.Success.Should().BeTrue();
        verifier.Verify(x => x.Verify(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public static async Task Validate_FullAndVerifierRejects_Fails()
    {
        // Arrange
        var service = CreateService(EmailVerificationResult.Invalid);

        // Act
        var result = await service.Validate("user@example.com", EmailValidationLevel.Full);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email address could not be verified");
    }

    [Test]
    public static async Task Validate_FullAndVerifierInconclusive_Passes()
    {
        // Arrange - an exhausted quota, an outage or no configured provider all land here, and none of
        // them may stop somebody signing up. This is the behaviour the whole design hangs on.
        var service = CreateService(EmailVerificationResult.Inconclusive);

        // Act
        var result = await service.Validate("user@example.com", EmailValidationLevel.Full);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task Validate_MalformedAddress_NeverReachesTheVerifier()
    {
        // Arrange - format is checked first, so an obviously broken address costs no credit.
        var verifier = new Mock<IEmailVerifier>();
        var service = new EmailValidationService(verifier.Object);

        // Act
        await service.Validate("not-an-email", EmailValidationLevel.Full);

        // Assert
        verifier.Verify(x => x.Verify(It.IsAny<string>()), Times.Never);
    }

    private static EmailValidationService CreateService(
        EmailVerificationResult result = EmailVerificationResult.Inconclusive)
    {
        if (result == EmailVerificationResult.Inconclusive)
        {
            return new EmailValidationService(new InconclusiveEmailVerifier());
        }

        var verifier = new Mock<IEmailVerifier>();
        verifier.Setup(x => x.Verify(It.IsAny<string>())).ReturnsAsync(result);
        return new EmailValidationService(verifier.Object);
    }
}

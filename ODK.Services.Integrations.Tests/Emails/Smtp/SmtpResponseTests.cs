using FluentAssertions;
using ODK.Services.Integrations.Emails.Smtp;

namespace ODK.Services.Integrations.Tests.Emails.Smtp;

[Parallelizable]
public static class SmtpResponseTests
{
    [Test]
    public static void Parse_200()
    {
        // Act
        var response = SmtpResponse.Parse("2.0.0 OK: queued as <51FZFPR0WQU4.8JZNXNWUTGR83@win6042>");

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be("OK");
        response.ExternalId.Should().Be("51FZFPR0WQU4.8JZNXNWUTGR83@win6042");
    }

    [Test]
    public static void Parse_ValidStatusFormat_NoExternalId()
    {
        // Act
        var response = SmtpResponse.Parse("1.0.0 blah");

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().Be("blah");
        response.ExternalId.Should().BeNull();
    }

    [Test]
    public static void Parse_InvalidStatusFormat()
    {
        // Act
        var response = SmtpResponse.Parse("123 blah");

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().BeNull();
        response.ExternalId.Should().BeNull();
    }
}

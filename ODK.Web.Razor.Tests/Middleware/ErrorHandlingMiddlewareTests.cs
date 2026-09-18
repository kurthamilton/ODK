using FluentAssertions;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Data.Core;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Middleware;
using OdkHttpRequest = ODK.Services.Logging.HttpRequest;

namespace ODK.Web.Razor.Tests.Middleware;

[Parallelizable]
public static class ErrorHandlingMiddlewareTests
{
    [Test]
    public static async Task InvokeAsync_ClientDisconnected_LogsNothing()
    {
        // Arrange
        var exception = new ConnectionResetException("The client has disconnected");

        // Act
        var loggingService = await Invoke(exception);

        // Assert
        VerifyWarned(loggingService, Times.Never());
        VerifyLoggedError(loggingService, Times.Never());
    }

    [Test]
    public static async Task InvokeAsync_NotAuthenticated_LogsAWarning()
    {
        // Arrange
        var exception = new OdkNotAuthenticatedException();

        // Act
        var loggingService = await Invoke(exception);

        // Assert
        VerifyWarned(loggingService, Times.Once());
        VerifyLoggedError(loggingService, Times.Never());
    }

    [Test]
    public static async Task InvokeAsync_NotAuthorized_LogsAnError()
    {
        // Arrange - a member refused access they should have had is a bug, so this one stays an error.
        var exception = new OdkNotAuthorizedException();

        // Act
        var loggingService = await Invoke(exception);

        // Assert
        VerifyLoggedError(loggingService, Times.Once());
        VerifyWarned(loggingService, Times.Never());
    }

    [Test]
    public static async Task InvokeAsync_UnexpectedException_LogsAnError()
    {
        // Arrange
        var exception = new InvalidOperationException("something went wrong");

        // Act
        var loggingService = await Invoke(exception);

        // Assert
        VerifyLoggedError(loggingService, Times.Once());
        VerifyWarned(loggingService, Times.Never());
    }

    [Test]
    public static void RedactSensitiveHeaders_IsCaseInsensitive()
    {
        var headers = new HeaderDictionary { { "authorization", "Bearer token" } };

        ErrorHandlingMiddleware.RedactSensitiveHeaders(headers)["authorization"]
            .Should().Be("[redacted]");
    }

    [Test]
    public static void RedactSensitiveHeaders_RedactsCredentialHeaders_KeepsOthers()
    {
        var headers = new HeaderDictionary
        {
            { "Cookie", "session=secret" },
            { "Authorization", "Bearer token" },
            { "User-Agent", "test-agent" }
        };

        var result = ErrorHandlingMiddleware.RedactSensitiveHeaders(headers);

        result["Cookie"].Should().Be("[redacted]");
        result["Authorization"].Should().Be("[redacted]");
        result["User-Agent"].Should().Be("test-agent");
    }

    /// <summary>
    /// Runs the middleware over a pipeline that throws <paramref name="exception"/> once, so the error
    /// page it re-executes afterwards completes as it would in the app.
    /// </summary>
    private static async Task<Mock<ILoggingService>> Invoke(Exception exception)
    {
        var loggingService = new Mock<ILoggingService>();

        var requestStore = new Mock<IRequestStore>();
        requestStore.Setup(x => x.Loaded).Returns(true);

        var odkRoutes = new Mock<IOdkRoutes>();
        odkRoutes
            .Setup(x => x.Error(It.IsAny<Chapter?>(), It.IsAny<int>()))
            .Returns("/error");

        var thrown = false;
        var middleware = new ErrorHandlingMiddleware(_ =>
        {
            if (thrown)
            {
                return Task.CompletedTask;
            }

            thrown = true;
            throw exception;
        });

        await middleware.InvokeAsync(
            new DefaultHttpContext(),
            loggingService.Object,
            requestStore.Object,
            new Mock<IUnitOfWork>().Object,
            odkRoutes.Object);

        return loggingService;
    }

    private static void VerifyLoggedError(Mock<ILoggingService> loggingService, Times times)
        => loggingService.Verify(
            x => x.Error(It.IsAny<Exception>(), It.IsAny<OdkHttpRequest>()),
            times);

    private static void VerifyWarned(Mock<ILoggingService> loggingService, Times times)
        => loggingService.Verify(
            x => x.Warn(It.IsAny<string>(), It.IsAny<IDictionary<string, string?>>()),
            times);
}

using FluentAssertions;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Exceptions;
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
        var (_, loggingService) = await Invoke(exception);

        // Assert
        VerifyWarned(loggingService, Times.Never());
        VerifyLoggedError(loggingService, Times.Never());
    }

    /// <summary>
    /// The case an unmatched route produces: nothing throws, the pipeline simply responds 404, and the
    /// middleware turns that into the error page. The status has to survive that, or every missing page
    /// on the site answers 200.
    /// </summary>
    [Test]
    public static async Task InvokeAsync_DownstreamResponds404_RespondsWith404()
    {
        // Act
        var (httpContext, _) = await Invoke(x => x.Response.StatusCode = 404);

        // Assert
        httpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public static async Task InvokeAsync_NotAuthenticated_LogsAWarning()
    {
        // Arrange
        var exception = new OdkNotAuthenticatedException();

        // Act
        var (_, loggingService) = await Invoke(exception);

        // Assert
        VerifyWarned(loggingService, Times.Once());
        VerifyLoggedError(loggingService, Times.Never());
    }

    [Test]
    public static async Task InvokeAsync_NotAuthenticated_RespondsWith401()
    {
        // Act
        var (httpContext, _) = await Invoke(new OdkNotAuthenticatedException());

        // Assert
        httpContext.Response.StatusCode.Should().Be(401);
    }

    [Test]
    public static async Task InvokeAsync_NotAuthorized_LogsAnError()
    {
        // Arrange - a member refused access they should have had is a bug, so this one stays an error.
        var exception = new OdkNotAuthorizedException();

        // Act
        var (_, loggingService) = await Invoke(exception);

        // Assert
        VerifyLoggedError(loggingService, Times.Once());
        VerifyWarned(loggingService, Times.Never());
    }

    [Test]
    public static async Task InvokeAsync_NotAuthorized_RespondsWith403()
    {
        // Act
        var (httpContext, _) = await Invoke(new OdkNotAuthorizedException());

        // Assert
        httpContext.Response.StatusCode.Should().Be(403);
    }

    /// <summary>
    /// The error page is chosen from the status code, so it has to be the one the request failed with
    /// rather than whatever the response carries by the time the page is resolved.
    /// </summary>
    [Test]
    public static async Task InvokeAsync_NotFound_RendersTheErrorPageForThatStatus()
    {
        // Arrange
        var odkRoutes = new Mock<IOdkRoutes>();
        odkRoutes
            .Setup(x => x.Error(It.IsAny<Chapter?>(), It.IsAny<int>()))
            .Returns((Chapter? _, int statusCode) => $"/error/{statusCode}");

        // Act
        var (httpContext, _) = await Invoke(_ => throw new OdkNotFoundException("nope"), odkRoutes);

        // Assert
        odkRoutes.Verify(x => x.Error(It.IsAny<Chapter?>(), 404), Times.Once());
        httpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public static async Task InvokeAsync_NotFound_RespondsWith404()
    {
        // Act
        var (httpContext, _) = await Invoke(new OdkNotFoundException("Path not found: /nope"));

        // Assert
        httpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public static async Task InvokeAsync_UnexpectedException_LogsAnError()
    {
        // Arrange
        var exception = new InvalidOperationException("something went wrong");

        // Act
        var (_, loggingService) = await Invoke(exception);

        // Assert
        VerifyLoggedError(loggingService, Times.Once());
        VerifyWarned(loggingService, Times.Never());
    }

    [Test]
    public static async Task InvokeAsync_UnexpectedException_RespondsWith500()
    {
        // Act
        var (httpContext, _) = await Invoke(new InvalidOperationException("something went wrong"));

        // Assert
        httpContext.Response.StatusCode.Should().Be(500);
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

    private static Task<(HttpContext HttpContext, Mock<ILoggingService> LoggingService)> Invoke(
        Exception exception)
        => Invoke(_ => throw exception);

    /// <summary>
    /// Runs the middleware over a pipeline that behaves as <paramref name="firstPass"/> says on the first
    /// call and completes on the second, so the error page it re-executes afterwards completes as it would
    /// in the app. The context comes back so a caller can assert on the response the middleware left.
    /// </summary>
    private static async Task<(HttpContext HttpContext, Mock<ILoggingService> LoggingService)> Invoke(
        Action<HttpContext> firstPass,
        Mock<IOdkRoutes>? odkRoutes = null)
    {
        var loggingService = new Mock<ILoggingService>();

        var requestStore = new Mock<IRequestStore>();
        requestStore.Setup(x => x.Loaded).Returns(true);

        if (odkRoutes == null)
        {
            odkRoutes = new Mock<IOdkRoutes>();
            odkRoutes
                .Setup(x => x.Error(It.IsAny<Chapter?>(), It.IsAny<int>()))
                .Returns("/error");
        }

        var called = false;
        var middleware = new ErrorHandlingMiddleware(context =>
        {
            if (called)
            {
                return Task.CompletedTask;
            }

            called = true;
            firstPass(context);
            return Task.CompletedTask;
        });

        var httpContext = new DefaultHttpContext();

        await middleware.InvokeAsync(
            httpContext,
            loggingService.Object,
            requestStore.Object,
            new Mock<IUnitOfWork>().Object,
            odkRoutes.Object);

        return (httpContext, loggingService);
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

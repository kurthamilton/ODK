using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ODK.Core.Web;
using ODK.Services.Logging;
using ODK.Web.Razor.Authentication;

namespace ODK.Web.Razor.Tests.Authentication;

[Parallelizable]
public static class LoggingAntiforgeryTests
{
    [Test]
    public static async Task ValidateRequestAsync_NoOriginAndNoSecFetchSite_DoesNotLog()
    {
        // Arrange - a client that sent neither header is not a browser, so the failure is bot noise.
        var logger = new Mock<ILogger<LoggingAntiforgery>>();
        var antiforgery = CreateAntiforgery(logger, out var httpContext);

        // Act
        var act = async () => await antiforgery.ValidateRequestAsync(httpContext);

        // Assert
        await act.Should().ThrowAsync<AntiforgeryValidationException>();
        VerifyLoggedError(logger, Times.Never());
    }

    [Test]
    public static async Task ValidateRequestAsync_PageHasNoHandlerForTheMethod_DoesNotLog()
    {
        // Arrange - a scanner POSTing /graphql matches the single-segment chapter route, which declares no
        // handlers at all, so it reaches antiforgery instead of 404ing. It sends an Origin, so the header
        // test above lets it through; nothing could have rendered a form posting here, so it is still noise.
        var logger = new Mock<ILogger<LoggingAntiforgery>>();
        var antiforgery = CreateAntiforgery(logger, out var httpContext);
        httpContext.Request.Headers["Origin"] = "https://example.com";
        SetPageEndpoint(httpContext, handlerHttpMethods: []);

        // Act
        var act = async () => await antiforgery.ValidateRequestAsync(httpContext);

        // Assert
        await act.Should().ThrowAsync<AntiforgeryValidationException>();
        VerifyLoggedError(logger, Times.Never());
    }

    [Test]
    public static async Task ValidateRequestAsync_PageHasAHandlerForTheMethod_Logs()
    {
        // Arrange - the page really does accept this POST, so a failed token is a genuine broken form.
        var logger = new Mock<ILogger<LoggingAntiforgery>>();
        var antiforgery = CreateAntiforgery(logger, out var httpContext);
        httpContext.Request.Headers["Origin"] = "https://example.com";
        SetPageEndpoint(httpContext, handlerHttpMethods: ["Get", "Post"]);

        // Act
        var act = async () => await antiforgery.ValidateRequestAsync(httpContext);

        // Assert
        await act.Should().ThrowAsync<AntiforgeryValidationException>();
        VerifyLoggedError(logger, Times.Once());
    }

    [Test]
    public static async Task ValidateRequestAsync_NoOriginButHasSecFetchSite_Logs()
    {
        // Arrange - one header is enough to say a browser sent this, so it stays an error.
        var logger = new Mock<ILogger<LoggingAntiforgery>>();
        var antiforgery = CreateAntiforgery(logger, out var httpContext);
        httpContext.Request.Headers["Sec-Fetch-Site"] = "same-origin";

        // Act
        var act = async () => await antiforgery.ValidateRequestAsync(httpContext);

        // Assert
        await act.Should().ThrowAsync<AntiforgeryValidationException>();
        VerifyLoggedError(logger, Times.Once());
    }

    [Test]
    public static async Task ValidateRequestAsync_NoRefererButHasOrigin_StillLogs()
    {
        // Arrange - Referer is deliberately not part of the bot test: privacy tooling and corporate
        // proxies strip it from genuine requests, so its absence alone must not suppress the error.
        var logger = new Mock<ILogger<LoggingAntiforgery>>();
        var antiforgery = CreateAntiforgery(logger, out var httpContext);
        httpContext.Request.Headers["Origin"] = "https://example.com";

        // Act
        var act = async () => await antiforgery.ValidateRequestAsync(httpContext);

        // Assert
        await act.Should().ThrowAsync<AntiforgeryValidationException>();
        VerifyLoggedError(logger, Times.Once());
    }

    [Test]
    public static async Task ValidateRequestAsync_Valid_DoesNotLog()
    {
        // Arrange
        var logger = new Mock<ILogger<LoggingAntiforgery>>();
        var inner = new Mock<IAntiforgery>();
        inner.Setup(x => x.ValidateRequestAsync(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var antiforgery = new LoggingAntiforgery(inner.Object, logger.Object);

        // Act
        await antiforgery.ValidateRequestAsync(CreateHttpContext());

        // Assert
        VerifyLoggedError(logger, Times.Never());
    }

    [Test]
    public static async Task ValidateRequestAsync_RequestAborted_DoesNotLog()
    {
        // Arrange - the client went away mid-request, so no token was ever read and nothing was validated.
        var logger = new Mock<ILogger<LoggingAntiforgery>>();
        var antiforgery = CreateAntiforgery(logger, out var httpContext);
        httpContext.Request.Headers["Origin"] = "https://example.com";
        httpContext.RequestAborted = new CancellationToken(canceled: true);

        // Act
        var act = async () => await antiforgery.ValidateRequestAsync(httpContext);

        // Assert
        await act.Should().ThrowAsync<AntiforgeryValidationException>();
        VerifyLoggedError(logger, Times.Never());
    }

    [Test]
    public static async Task ValidateRequestAsync_ClientDisconnectedBeforeTheAbortTokenFired_DoesNotLog()
    {
        // Arrange - IIS reports a disconnected client as the inner exception of the read that failed, and
        // it can arrive before the abort token is raised.
        var logger = new Mock<ILogger<LoggingAntiforgery>>();
        var antiforgery = CreateAntiforgery(
            logger,
            out var httpContext,
            out _,
            new ConnectionResetException("The client has disconnected"));
        httpContext.Request.Headers["Origin"] = "https://example.com";

        // Act
        var act = async () => await antiforgery.ValidateRequestAsync(httpContext);

        // Assert
        await act.Should().ThrowAsync<AntiforgeryValidationException>();
        VerifyLoggedError(logger, Times.Never());
    }

    [Test]
    public static async Task ValidateRequestAsync_TokenValidForAnAnonymousVisitor_DoesNotLog()
    {
        // Arrange - the form was rendered before the member signed in, so its token is bound to nobody
        // while the member posting it is somebody. The inner validation answers for whichever principal it
        // is handed, so it passes only if the probe really did swap an anonymous one in.
        var logger = new Mock<ILogger<LoggingAntiforgery>>();
        var antiforgery = CreateAntiforgery(logger, out var httpContext, out var inner);
        var user = SignedInMember();
        httpContext.Request.Headers["Origin"] = "https://example.com";
        httpContext.User = user;
        inner
            .Setup(x => x.IsRequestValidAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync((HttpContext x) => x.User.Identity?.IsAuthenticated != true);

        // Act
        var act = async () => await antiforgery.ValidateRequestAsync(httpContext);

        // Assert
        await act.Should().ThrowAsync<AntiforgeryValidationException>();
        VerifyLoggedError(logger, Times.Never());
        httpContext.User.Should().BeSameAs(user);
    }

    [Test]
    public static async Task ValidateRequestAsync_TokenInvalidForAnyVisitor_Logs()
    {
        // Arrange - a token no principal can validate is a broken or forged form rather than a member who
        // signed in while the page was open.
        var logger = new Mock<ILogger<LoggingAntiforgery>>();
        var antiforgery = CreateAntiforgery(logger, out var httpContext, out var inner);
        httpContext.Request.Headers["Origin"] = "https://example.com";
        httpContext.User = SignedInMember();
        inner
            .Setup(x => x.IsRequestValidAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(false);

        // Act
        var act = async () => await antiforgery.ValidateRequestAsync(httpContext);

        // Assert
        await act.Should().ThrowAsync<AntiforgeryValidationException>();
        VerifyLoggedError(logger, Times.Once());
    }

    private static LoggingAntiforgery CreateAntiforgery(
        Mock<ILogger<LoggingAntiforgery>> logger, out HttpContext httpContext)
        => CreateAntiforgery(logger, out httpContext, out _);

    private static LoggingAntiforgery CreateAntiforgery(
        Mock<ILogger<LoggingAntiforgery>> logger,
        out HttpContext httpContext,
        out Mock<IAntiforgery> inner,
        Exception? innerException = null)
    {
        httpContext = CreateHttpContext();

        inner = new Mock<IAntiforgery>();
        const string message = "The required antiforgery token was not provided.";

        inner.Setup(x => x.ValidateRequestAsync(It.IsAny<HttpContext>()))
            .ThrowsAsync(innerException != null
                ? new AntiforgeryValidationException(message, innerException)
                : new AntiforgeryValidationException(message));

        return new LoggingAntiforgery(inner.Object, logger.Object);
    }

    private static HttpContext CreateHttpContext()
    {
        var loggingService = new Mock<ILoggingService>();
        loggingService
            .Setup(x => x.IgnoreException(It.IsAny<Exception>(), It.IsAny<IHttpRequestContext>()))
            .Returns(false);

        var services = new ServiceCollection();
        services.AddSingleton(loggingService.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = "/account/login";

        return httpContext;
    }

    private static void SetPageEndpoint(HttpContext httpContext, string[] handlerHttpMethods)
    {
        var descriptor = new CompiledPageActionDescriptor
        {
            HandlerMethods = handlerHttpMethods
                .Select(x => new HandlerMethodDescriptor { HttpMethod = x })
                .ToList()
        };

        httpContext.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(descriptor),
            "test-page"));
    }

    private static ClaimsPrincipal SignedInMember() => new(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())],
        authenticationType: "test"));

    // ILogger.LogError is an extension over the non-generic Log, so the verification has to match that
    // underlying call rather than the extension method.
    private static void VerifyLoggedError(Mock<ILogger<LoggingAntiforgery>> logger, Times times)
        => logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            times);
}

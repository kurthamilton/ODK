using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Logging;
using Serilog;

namespace ODK.Services.Tests.Logging;

[Parallelizable]
public static class LoggingServiceTests
{
    [TestCase("/abc/", "/abc", ExpectedResult = true)]
    [TestCase("/abc/", "/abcd", ExpectedResult = false)]
    [TestCase("/abc*", "/abcd", ExpectedResult = true)]
    [TestCase("/abc/", "/x/abc", ExpectedResult = false)]
    [TestCase("*/abc/", "/x/abc", ExpectedResult = true)]
    [TestCase("*/index.html", "/index.html", ExpectedResult = true)]
    [TestCase("*/index.html", "/home/index.html", ExpectedResult = true)]
    public static bool IgnoreUnknownRequestPath_IgnorePaths_Wildcards(string config, string path)
    {
        // Arrange
        var settings = CreateSettings(
            ignoreUnknownPaths: config.Split(','));

        var service = CreateService(settings: settings);

        var request = CreateMockHttpRequestContext(path: path);

        // Act
        var result = service.IgnoreUnknownRequestPath(request);

        // Assert
        return result;
    }

    [TestCase("*spider*", "this is a spider usergent", ExpectedResult = true)]
    [TestCase("spider*", "this is a spider usergent", ExpectedResult = false)]
    [TestCase("*spider", "this is a spider usergent", ExpectedResult = false)]
    public static bool IgnoreUnknownRequestPath_IgnoreUserAgents(string config, string userAgent)
    {
        // Arrange
        var settings = CreateSettings(
            ignoreUnkownPathUserAgents: config.Split(','));

        var service = CreateService(settings: settings);

        var request = CreateMockHttpRequestContext(userAgent: userAgent);

        // Act
        var result = service.IgnoreUnknownRequestPath(request);

        // Assert
        return result;
    }

    private static IHttpRequestContext CreateMockHttpRequestContext(
        string? path = null,
        string? userAgent = null)
    {
        var mock = new Mock<IHttpRequestContext>();

        mock.Setup(x => x.RequestPath)
            .Returns(path ?? string.Empty);

        mock.Setup(x => x.UserAgent)
            .Returns(userAgent ?? string.Empty);

        return mock.Object;
    }

    private static LoggingService CreateService(
        LoggingServiceSettings? settings = null)
    {
        return new LoggingService(
            Mock.Of<ILogger>(),
            Mock.Of<IUnitOfWorkFactory>(),
            Mock.Of<IUnitOfWork>(),
            settings ?? CreateSettings());
    }

    private static LoggingServiceSettings CreateSettings(
        IEnumerable<string>? ignoreUnknownPaths = null,
        IEnumerable<string>? ignoreUnknownPatterns = null,
        IEnumerable<string>? ignoreUnkownPathUserAgents = null)
    {
        return new LoggingServiceSettings
        {
            IgnoreUnknownPaths = ignoreUnknownPaths?.ToArray() ?? [],
            IgnoreUnknownPathPatterns = ignoreUnknownPatterns?.ToArray() ?? [],
            IgnoreUnknownPathUserAgents = ignoreUnkownPathUserAgents?.ToArray() ?? []
        };
    }
}
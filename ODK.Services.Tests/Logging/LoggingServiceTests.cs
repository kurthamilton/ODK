using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ODK.Core.Exceptions;
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
    [TestCase("*", "/anything", ExpectedResult = true)]
    public static bool IgnoreUnknownRequestPath_IgnorePaths_Wildcards(string config, string path)
    {
        // Arrange
        var settings = CreateSettings(
            ignoreUnknownPaths: config.Split(','));

        var service = CreateService(settings: settings);

        var request = CreateMockHttpRequestContext(path: path);

        // Act
        var result = service.IgnoreException(new OdkNotFoundException(), request);

        // Assert
        return result;
    }

    [TestCase("*spider*", "this is a spider usergent", ExpectedResult = true)]
    [TestCase("spider*", "this is a spider usergent", ExpectedResult = false)]
    [TestCase("*spider", "this is a spider usergent", ExpectedResult = false)]
    [TestCase("*", "any user agent", ExpectedResult = true)]
    public static bool IgnoreUnknownRequestPath_IgnoreUserAgents(string config, string userAgent)
    {
        // Arrange
        var settings = CreateSettings(
            ignoreUnkownPathUserAgents: config.Split(','));

        var service = CreateService(settings: settings);

        var request = CreateMockHttpRequestContext(userAgent: userAgent);

        // Act
        var result = service.IgnoreException(new OdkNotFoundException(), request);

        // Assert
        return result;
    }

    [TestCase("X-Bot", "*", "X-Bot", "anything", ExpectedResult = true)]
    [TestCase("X-Bot", "crawler", "X-Bot", "crawler", ExpectedResult = true)]
    [TestCase("X-Bot", "crawler", "X-Bot", "a crawler", ExpectedResult = false)]
    [TestCase("X-Bot", "*crawler*", "X-Bot", "a crawler here", ExpectedResult = true)]
    [TestCase("X-Bot", "crawler*", "X-Bot", "crawler here", ExpectedResult = true)]
    [TestCase("X-Bot", "*crawler", "X-Bot", "a crawler", ExpectedResult = true)]
    [TestCase("X-Bot", "crawler", "X-Other", "crawler", ExpectedResult = false)]
    [TestCase("X-Bot", "crawler", "X-Bot", "spider", ExpectedResult = false)]
    public static bool IgnoreException_IgnoreHeaders(
        string ruleName, string ruleValue, string headerName, string headerValue)
    {
        // Arrange
        var settings = CreateSettings(
            ignoreHeaders: new Dictionary<string, string[]> { [ruleName] = [ruleValue] });

        var service = CreateService(settings: settings);

        var request = CreateMockHttpRequestContext(
            headers: new Dictionary<string, string[]> { [headerName] = [headerValue] });

        // Act
        var result = service.IgnoreException(new OdkNotFoundException(), request);

        // Assert
        return result;
    }

    [TestCase("x-bot", "X-BOT", ExpectedResult = true)]
    [TestCase("X-BOT", "x-bot", ExpectedResult = true)]
    public static bool IgnoreException_IgnoreHeaders_NameCaseIsIgnored(string ruleName, string headerName)
    {
        /* Arrange - the request's own header collection is case-insensitive, so a rule written in any casing has
           to behave the same. The match compares keys rather than looking them up, so this holds even for a
           dictionary built with the default comparer, as this one is. */
        var settings = CreateSettings(
            ignoreHeaders: new Dictionary<string, string[]> { [ruleName] = ["crawler"] });

        var service = CreateService(settings: settings);

        var request = CreateMockHttpRequestContext(
            headers: new Dictionary<string, string[]> { [headerName] = ["crawler"] });

        // Act
        var result = service.IgnoreException(new OdkNotFoundException(), request);

        // Assert
        return result;
    }

    [Test]
    public static void IgnoreException_IgnoreHeaders_MatchesAnyOfSeveralValues()
    {
        // Arrange
        var settings = CreateSettings(
            ignoreHeaders: new Dictionary<string, string[]> { ["X-Bot"] = ["spider", "crawler", "worm"] });

        var service = CreateService(settings: settings);

        var request = CreateMockHttpRequestContext(
            headers: new Dictionary<string, string[]> { ["X-Bot"] = ["crawler"] });

        // Act
        var result = service.IgnoreException(new OdkNotFoundException(), request);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public static void IgnoreException_IgnoreHeaders_MatchesAnyOfARepeatedHeadersValues()
    {
        /* Arrange - a header can arrive more than once, which is why the context carries every value rather
           than one joined string: joined, an exact-value rule like this would not match. */
        var settings = CreateSettings(
            ignoreHeaders: new Dictionary<string, string[]> { ["X-Bot"] = ["crawler"] });

        var service = CreateService(settings: settings);

        var request = CreateMockHttpRequestContext(
            headers: new Dictionary<string, string[]> { ["X-Bot"] = ["spider", "crawler"] });

        // Act
        var result = service.IgnoreException(new OdkNotFoundException(), request);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public static void IgnoreException_NoRuleHeaders_DoesNotReadTheRequestHeaders()
    {
        /* Arrange - a rule naming no header must not touch the collection, so every other caller of
           IgnoreException is unaffected by headers existing at all. */
        var settings = CreateSettings(ignoreUnknownPaths: ["/abc/"]);

        var service = CreateService(settings: settings);

        var mock = new Mock<IHttpRequestContext>();
        mock.Setup(x => x.RequestPath).Returns("/abc");
        mock.Setup(x => x.UserAgent).Returns(string.Empty);
        mock.Setup(x => x.Headers).Throws(new AssertionException("Headers should not be read"));

        // Act
        var result = service.IgnoreException(new OdkNotFoundException(), mock.Object);

        // Assert
        Assert.That(result, Is.True);
    }

    private static IHttpRequestContext CreateMockHttpRequestContext(
        string? path = null,
        string? userAgent = null,
        IReadOnlyDictionary<string, string[]>? headers = null)
    {
        var mock = new Mock<IHttpRequestContext>();

        mock.Setup(x => x.Headers)
            .Returns(headers ?? new Dictionary<string, string[]>());

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
        Dictionary<string, string[]>? ignoreHeaders = null,
        IEnumerable<string>? ignoreUnknownPaths = null,
        IEnumerable<string>? ignoreUnknownPatterns = null,
        IEnumerable<string>? ignoreUnkownPathUserAgents = null)
    {
        var ignoreExceptions = new List<IgnoreExceptionRule>();

        // One rule holding every header, since a rule listing several headers is the case worth covering.
        if (ignoreHeaders != null)
        {
            ignoreExceptions.Add(new IgnoreExceptionRule
            {
                Exceptions = [nameof(OdkNotFoundException)],
                Headers = ignoreHeaders
            });
        }

        if (ignoreUnknownPaths != null)
        {
            ignoreExceptions.AddRange(ignoreUnknownPaths.Select(x => new IgnoreExceptionRule
            {
                Exceptions = new[] { nameof(OdkNotFoundException) },
                Paths = ignoreUnknownPaths.ToArray()
            }));
        }

        if (ignoreUnknownPatterns != null)
        {
            ignoreExceptions.AddRange(ignoreUnknownPatterns.Select(x => new IgnoreExceptionRule
            {
                Exceptions = new[] { nameof(OdkNotFoundException) },
                PathPatterns = ignoreUnknownPatterns.ToArray()
            }));
        }

        if (ignoreUnkownPathUserAgents != null)
        {
            ignoreExceptions.AddRange(ignoreUnkownPathUserAgents.Select(x => new IgnoreExceptionRule
            {
                Exceptions = new[] { nameof(OdkNotFoundException) },
                UserAgents = ignoreUnkownPathUserAgents.ToArray()
            }));
        }

        return new LoggingServiceSettings
        {
            IgnoreExceptions = ignoreExceptions
        };
    }
}
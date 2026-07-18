using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using ODK.Web.Razor.Middleware;

namespace ODK.Web.Razor.Tests.Middleware;

[Parallelizable]
public static class ErrorHandlingMiddlewareTests
{
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
}

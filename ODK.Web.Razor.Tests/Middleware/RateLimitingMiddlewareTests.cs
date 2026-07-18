using FluentAssertions;
using NUnit.Framework;
using ODK.Web.Razor.Middleware;

namespace ODK.Web.Razor.Tests.Middleware;

[Parallelizable]
public static class RateLimitingMiddlewareTests
{
    [TestCase("wp-login.php", "wp-login.php", true)]
    [TestCase("wp-login.php", "index.php", false)]
    [TestCase("WP-LOGIN.PHP", "wp-login.php", true)] // exact, case-insensitive
    [TestCase("*.php", "shell.php", true)] // leading wildcard -> EndsWith
    [TestCase("*.php", "shell.html", false)]
    [TestCase("admin*", "admin/config", true)] // trailing wildcard -> StartsWith
    [TestCase("admin*", "user/admin", false)]
    [TestCase("*sql*", "x-sql-y", true)] // both wildcards -> Contains
    [TestCase("*sql*", "harmless", false)]
    public static void MatchesConfigRule_ReturnsExpected(string rule, string value, bool expected)
        => RateLimitingMiddleware.MatchesConfigRule(rule, value).Should().Be(expected);
}

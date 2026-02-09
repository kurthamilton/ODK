using NUnit.Framework;
using ODK.Core.Utils;

namespace ODK.Core.Tests.Utils;

[Parallelizable]
public static class UrlUtilsTests
{
    [TestCase("/home/", ExpectedResult = "/home/")]
    [TestCase("/home", ExpectedResult = "/home/")]
    [TestCase("home/", ExpectedResult = "/home/")]
    [TestCase("/index.html", ExpectedResult = "/index.html")]
    [TestCase("index.html", ExpectedResult = "/index.html")]
    public static string NormalisePath(string path) => UrlUtils.NormalisePath(path);

    [TestCase("Some String", ExpectedResult = "some-string")]
    [TestCase("C# .NET", ExpectedResult = "c-net")]
    [TestCase("Rock & Roll", ExpectedResult = "rock-and-roll")]
    [TestCase("Be@One", ExpectedResult = "be-at-one")]
    [TestCase("One+", ExpectedResult = "one-plus")]
    public static string Slugify(string s) => UrlUtils.Slugify(s);
}
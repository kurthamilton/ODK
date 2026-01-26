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
}
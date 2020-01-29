using System.Collections.Generic;
using NUnit.Framework;
using ODK.Core.Utils;

namespace ODK.Core.Tests.Utils
{
    public static class StringUtilsTests
    {
        [TestCase("a2c")]
        [TestCase(".a.2.c.")]
        [TestCase("\\a.2.c/")]
        [TestCase("\\a.2.c/")]
        [TestCase("#a.2.c###################")]
        public static void AlphaNumericReturnsAlphaNumericString(string text)
        {
            text = StringUtils.AlphaNumeric(text);

            Assert.AreEqual("a2c", text);
        }

        [Test]
        public static void Interpolate_ReplacesTokens()
        {
            string text = "The {a} brown {b}";

            string result = StringUtils.Interpolate(text, new Dictionary<string, string>
            {
                {"a", "quick"},
                {"b", "fox"}
            });

            Assert.AreEqual("The quick brown fox", result);
        }

        [Test]
        public static void Interpolate_ReplacesAllInstances()
        {
            string text = "The {a} brown {a} {b}";

            string result = StringUtils.Interpolate(text, new Dictionary<string, string>
            {
                {"a", "quick"},
                {"b", "fox"}
            });

            Assert.AreEqual("The quick brown quick fox", result);
        }

        [Test]
        public static void Interpolate_IgnoresMissingTokenValues()
        {
            string text = "The {a} brown {b}";

            string result = StringUtils.Interpolate(text, new Dictionary<string, string>
            {
                {"a", "quick"}
            });

            Assert.AreEqual("The quick brown {b}", result);
        }

        [Test]
        public static void Tokens_ReturnsMultipleTokens()
        {
            IEnumerable<string> tokens = StringUtils.Tokens("This {a} a {b} {c}");

            CollectionAssert.AreEqual(new [] { "a", "b", "c" }, tokens);
        }
    }
}

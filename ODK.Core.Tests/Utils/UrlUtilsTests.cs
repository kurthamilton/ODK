using System;
using System.Collections.Generic;
using FluentAssertions;
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

    [Test]
    public static void SlugifyUnique_LongName_TruncatesLeavingRoomForTheVersion()
    {
        // Arrange
        var name = new string('a', 50);
        var taken = new HashSet<string> { new('a', 10) };

        // Act
        var result = UrlUtils.SlugifyUnique(name, taken, maxLength: 10);

        // Assert
        result.Should().Be(new string('a', 8) + "-2");
        result!.Length.Should().Be(10);
    }

    [Test]
    public static void SlugifyUnique_NothingSluggable_ReturnsNull()
    {
        // Arrange
        var taken = new HashSet<string>();

        // Act
        var result = UrlUtils.SlugifyUnique("!!! ???", taken, maxLength: 100);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static void SlugifyUnique_TakenDiffersOnlyByCase_StillVersions()
    {
        // Arrange
        // The caller supplies the comparer; SQL Server's default collation is case insensitive, so a
        // case-insensitive set must be honoured or the future unique index would reject the result.
        var taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "THE-OAK" };

        // Act
        var result = UrlUtils.SlugifyUnique("The Oak", taken, maxLength: 100);

        // Assert
        result.Should().Be("the-oak-2");
    }

    [Test]
    public static void SlugifyUnique_TruncationEndsOnSeparator_TrimsTrailingHyphen()
    {
        // Arrange
        var taken = new HashSet<string>();

        // Act
        // "the-oak-tree" cut at 8 chars is "the-oak-", which must not keep its trailing separator.
        var result = UrlUtils.SlugifyUnique("The Oak Tree", taken, maxLength: 8);

        // Assert
        result.Should().Be("the-oak");
    }

    [TestCase("The Oak", ExpectedResult = "the-oak-4")]
    [TestCase("The Elm", ExpectedResult = "the-elm")]
    public static string? SlugifyUnique_VersionsPastEveryTakenSlug(string name)
    {
        var taken = new HashSet<string> { "the-oak", "the-oak-2", "the-oak-3" };
        return UrlUtils.SlugifyUnique(name, taken, maxLength: 100);
    }
}
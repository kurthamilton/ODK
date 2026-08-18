using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Workflows.Tests.Fakes;

namespace ODK.Core.Workflows.Tests;

[Parallelizable]
public static class NegatedGuardTests
{
    [Test]
    public static void Description_Guard_ReadsAsTheOppositeOfTheGuardItNegates()
    {
        // Arrange
        var guard = Guard.Not(new FlagIsSet());

        // Act
        var result = guard.Description;

        // Assert
        result.Should().Be("not the flag is set");
    }

    [TestCase(true, ExpectedResult = false)]
    [TestCase(false, ExpectedResult = true)]
    public static bool IsSatisfied_Guard_ReturnsTheOppositeOfTheGuardItNegates(bool flag)
    {
        // Arrange
        var guard = Guard.Not(new FlagIsSet());
        var context = new SampleContext { Flag = flag };

        // Act
        return guard.IsSatisfied(context);
    }
}

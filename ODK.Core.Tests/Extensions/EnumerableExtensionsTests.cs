using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Extensions;

namespace ODK.Core.Tests.Extensions;

[Parallelizable]
public static class EnumerableExtensionsTests
{
    [Test]
    public static void EquivalentTo_SameOrder_ReturnsTrue()
    {
        // Arrange
        var first = new[]
        {
            "a", "b", "c"
        };

        var second = new[]
        {
            "a", "b", "c"
        };

        // Act
        var result = first.EquivalentTo(second, null);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void EquivalentTo_DifferentOrder_ReturnsTrue()
    {
        // Arrange
        var first = new[]
        {
            "a", "b", "c"
        };

        var second = new[]
        {
            "c", "b", "a"
        };

        // Act
        var result = first.EquivalentTo(second, null);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void EquivalentTo_FailEqualityTest_ReturnsFalse()
    {
        // Arrange
        var first = new[]
        {
            "a", "b", "c"
        };

        var second = new[]
        {
            "A", "B", "C"
        };

        // Act
        var result = first.EquivalentTo(second, null);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void EquivalentTo_EqualityComparer_ReturnsTrue()
    {
        // Arrange
        var first = new[]
        {
            "a", "b", "c"
        };

        var second = new[]
        {
            "A", "B", "C"
        };

        // Act
        var result = first.EquivalentTo(second, StringComparer.OrdinalIgnoreCase);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void EquivalentTo_DifferentNumberOfElements_ReturnsFalse()
    {
        // Arrange
        var first = new[]
        {
            "a", "b", "c"
        };

        var second = new[]
        {
            "a", "b", "c", "d"
        };

        // Act
        var result = first.EquivalentTo(second, null);

        // Assert
        result.Should().BeFalse();
    }
}
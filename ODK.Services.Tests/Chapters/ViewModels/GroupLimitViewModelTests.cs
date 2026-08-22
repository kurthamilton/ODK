using FluentAssertions;
using NUnit.Framework;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Tests.Chapters.ViewModels;

[Parallelizable]
public static class GroupLimitViewModelTests
{
    [Test]
    public static void CanCreate_LimitReached_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateGroupLimitViewModel(count: 1, limit: 1);

        // Act
        var result = viewModel.CanCreate;

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void CanCreate_NoLimit_ReturnsTrue()
    {
        // Arrange
        var viewModel = CreateGroupLimitViewModel(count: 5, limit: null);

        // Act
        var result = viewModel.CanCreate;

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void CanCreate_UnderLimit_ReturnsTrue()
    {
        // Arrange
        var viewModel = CreateGroupLimitViewModel(count: 1, limit: 3);

        // Act
        var result = viewModel.CanCreate;

        // Assert
        result.Should().BeTrue();
    }

    [TestCase(0, 3, 3)]
    [TestCase(2, 3, 1)]
    [TestCase(3, 3, 0)]
    public static void Remaining_WithLimit_ReturnsGroupsLeft(int count, int limit, int expected)
    {
        // Arrange
        var viewModel = CreateGroupLimitViewModel(count, limit);

        // Act
        var result = viewModel.Remaining;

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public static void Remaining_CountExceedsLimit_ReturnsZero()
    {
        // Arrange
        var viewModel = CreateGroupLimitViewModel(count: 4, limit: 2);

        // Act
        var result = viewModel.Remaining;

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public static void Remaining_NoLimit_ReturnsNull()
    {
        // Arrange
        var viewModel = CreateGroupLimitViewModel(count: 2, limit: null);

        // Act
        var result = viewModel.Remaining;

        // Assert
        result.Should().BeNull();
    }

    private static GroupLimitViewModel CreateGroupLimitViewModel(int count, int? limit) => new()
    {
        Count = count,
        Limit = limit
    };
}

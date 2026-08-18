using System;
using FluentAssertions;
using NUnit.Framework;

namespace ODK.Core.Workflows.Tests;

[Parallelizable]
public static class WriteOnceTests
{
    [Test]
    public static void Value_NotYetWritten_IsNull()
    {
        // Arrange
        var slot = new WriteOnce<string>("The slot");

        // Act
        var result = slot.Value;

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static void Value_WrittenOnce_ReturnsWhatWasWritten()
    {
        // Arrange
        var slot = new WriteOnce<string>("The slot");

        // Act
        slot.Value = "written";

        // Assert
        slot.Value.Should().Be("written");
    }

    [Test]
    public static void Value_WrittenTwice_ThrowsNamingTheSlot()
    {
        // Arrange
        var slot = new WriteOnce<string>("The account the transition creates");
        slot.Value = "first";

        // Act
        var act = () => slot.Value = "second";

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The account the transition creates has already been set");
    }

    [Test]
    public static void Value_WrittenNullThenWrittenAgain_Throws()
    {
        /* Arrange - writing nothing still counts as written, so a slot cannot be reopened by assigning null and
           then filled by a second step. */
        var slot = new WriteOnce<string>("The slot");
        slot.Value = null;

        // Act
        var act = () => slot.Value = "second";

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}

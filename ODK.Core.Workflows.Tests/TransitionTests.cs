using System;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Workflows.Tests.Fakes;

namespace ODK.Core.Workflows.Tests;

[Parallelizable]
public static class TransitionTests
{
    [Test]
    public static void IsPermitted_EveryGuardSatisfied_ReturnsTrue()
    {
        // Arrange
        var transition = CreateTransition(new SampleGuard("the first holds", true), new SampleGuard("the second holds", true));

        // Act
        var result = transition.IsPermitted(new SampleContext());

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void IsPermitted_OneGuardNotSatisfied_ReturnsFalse()
    {
        // Arrange
        var transition = CreateTransition(new SampleGuard("the first holds", true), new SampleGuard("the second holds", false));

        // Act
        var result = transition.IsPermitted(new SampleContext());

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void IsPermitted_NoGuards_ReturnsTrue()
    {
        // Arrange
        var transition = CreateTransition();

        // Act
        var result = transition.IsPermitted(new SampleContext());

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void Label_NoGuards_ReadsAsTheTrigger()
    {
        // Arrange
        var transition = CreateTransition();

        // Act
        var result = transition.Label();

        // Assert
        result.Should().Be("Go");
    }

    [Test]
    public static void Label_Guards_ReadsAsTheTriggerAndEveryGuard()
    {
        // Arrange
        var transition = CreateTransition(new SampleGuard("the first holds", true), new SampleGuard("the second holds", true));

        // Act
        var result = transition.Label();

        // Assert
        result.Should().Be("Go [the first holds, the second holds]");
    }

    private static Transition<SampleState, SampleTrigger, SampleContext> CreateTransition(
        params IGuard<SampleContext>[] guards) => new()
    {
        From = SampleState.Start,
        Guards = guards,
        Steps = Array.Empty<StepDefinition>(),
        To = SampleState.Middle,
        Trigger = SampleTrigger.Go
    };
}

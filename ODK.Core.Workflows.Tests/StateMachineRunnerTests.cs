using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Workflows.Tests.Fakes;

namespace ODK.Core.Workflows.Tests;

[Parallelizable]
public static class StateMachineRunnerTests
{
    [Test]
    public static async Task Fire_PermittedTransition_RunsEveryStepInOrder()
    {
        // Arrange
        var definition = Definition(x => x
            .Then<DecisionStep>()
            .Then<WriteStep>()
            .Then<CommitStep>()
            .Then<ExternalEffectStep>());
        var context = new SampleContext { State = SampleState.Start };

        // Act
        var result = await Runner(definition).Fire(SampleTrigger.Go, context);

        // Assert
        result.Success.Should().BeTrue();
        context.Executed.Should().Equal(
            nameof(DecisionStep),
            nameof(WriteStep),
            nameof(CommitStep),
            nameof(ExternalEffectStep));
    }

    [Test]
    public static async Task Fire_PermittedTransition_ReturnsTheStateItMovedTo()
    {
        // Arrange
        var definition = Definition();
        var context = new SampleContext { State = SampleState.Start };

        // Act
        var result = await Runner(definition).Fire(SampleTrigger.Go, context);

        // Assert
        result.Success.Should().BeTrue();
        result.From.Should().Be(SampleState.Start);
        result.To.Should().Be(SampleState.Middle);
    }

    [Test]
    public static async Task Fire_StepFails_StopsAndReportsTheFailure()
    {
        // Arrange
        var definition = Definition(x => x
            .Then<DecisionStep>()
            .Then<FailingStep>()
            .Then<WriteStep>());
        var context = new SampleContext { State = SampleState.Start };

        // Act
        var result = await Runner(definition).Fire(SampleTrigger.Go, context);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(FailingStep.FailureMessage);
        context.Executed.Should().Equal(nameof(DecisionStep), nameof(FailingStep));
    }

    [Test]
    public static async Task Fire_StepFails_StaysInTheStateItStartedIn()
    {
        // Arrange
        var definition = Definition(x => x.Then<FailingStep>());
        var context = new SampleContext { State = SampleState.Start };

        // Act
        var result = await Runner(definition).Fire(SampleTrigger.Go, context);

        // Assert
        result.From.Should().Be(SampleState.Start);
        result.To.Should().Be(SampleState.Start);
    }

    [Test]
    public static async Task Fire_StepStops_SucceedsWithoutRunningTheStepsAfterIt()
    {
        // Arrange
        var definition = Definition(x => x
            .Then<DecisionStep>()
            .Then<StoppingStep>()
            .Then<WriteStep>());
        var context = new SampleContext { State = SampleState.Start };

        // Act
        var result = await Runner(definition).Fire(SampleTrigger.Go, context);

        // Assert
        result.Success.Should().BeTrue();
        result.To.Should().Be(SampleState.Middle);
        context.Executed.Should().Equal(nameof(DecisionStep), nameof(StoppingStep));
    }

    [Test]
    public static async Task Fire_TriggerNotValidFromTheCurrentState_Fails()
    {
        // Arrange
        var definition = Definition();
        var context = new SampleContext { State = SampleState.Start };

        // Act
        var result = await Runner(definition).Fire(SampleTrigger.Stop, context);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Stop is not permitted from Start");
    }

    [Test]
    public static async Task Fire_GuardIsNotSatisfied_TakesTheOtherTransition()
    {
        // Arrange
        var definition = Guarded();
        var context = new SampleContext { State = SampleState.Start, Flag = false };

        // Act
        var result = await Runner(definition).Fire(SampleTrigger.Go, context);

        // Assert
        result.To.Should().Be(SampleState.End);
        context.Executed.Should().Equal(nameof(WriteStep));
    }

    [Test]
    public static async Task Fire_GuardIsSatisfied_TakesTheGuardedTransition()
    {
        // Arrange
        var definition = Guarded();
        var context = new SampleContext { State = SampleState.Start, Flag = true };

        // Act
        var result = await Runner(definition).Fire(SampleTrigger.Go, context);

        // Assert
        result.To.Should().Be(SampleState.Middle);
        context.Executed.Should().Equal(nameof(DecisionStep));
    }

    [Test]
    public static async Task Fire_MoreThanOneTransitionPermitted_Throws()
    {
        // Arrange
        var definition = StateMachine
            .Define<SampleState, SampleTrigger, SampleContext>("Sample")
            .StartingAt(SampleState.Start)
            .Transition(
                SampleState.Start,
                SampleTrigger.Go,
                SampleState.Middle,
                x => x.When(new SampleGuard("the first condition holds", true)))
            .Transition(
                SampleState.Start,
                SampleTrigger.Go,
                SampleState.End,
                x => x.When(new SampleGuard("the second condition holds", true)))
            .Transition(SampleState.Middle, SampleTrigger.Stop, SampleState.End)
            .Build();
        var context = new SampleContext { State = SampleState.Start };

        // Act
        var act = async () => await Runner(definition).Fire(SampleTrigger.Go, context);

        // Assert
        await act.Should().ThrowAsync<StateMachineDefinitionException>()
            .WithMessage("*matches more than one transition*");
    }

    private static StateMachineDefinition<SampleState, SampleTrigger, SampleContext> Definition(
        Action<TransitionBuilder<SampleContext>>? configure = null) => StateMachine
        .Define<SampleState, SampleTrigger, SampleContext>("Sample")
        .StartingAt(SampleState.Start)
        .Transition(SampleState.Start, SampleTrigger.Go, SampleState.Middle, configure)
        .Transition(SampleState.Middle, SampleTrigger.Stop, SampleState.End)
        .Build();

    private static StateMachineDefinition<SampleState, SampleTrigger, SampleContext> Guarded() => StateMachine
        .Define<SampleState, SampleTrigger, SampleContext>("Sample")
        .StartingAt(SampleState.Start)
        .Transition(
            SampleState.Start,
            SampleTrigger.Go,
            SampleState.Middle,
            x => x.When(new FlagIsSet()).Then<DecisionStep>())
        .Transition(
            SampleState.Start,
            SampleTrigger.Go,
            SampleState.End,
            x => x.When(Guard.Not(new FlagIsSet())).Then<WriteStep>())
        .Transition(SampleState.Middle, SampleTrigger.Stop, SampleState.End)
        .Build();

    private static StateMachineRunner<SampleState, SampleTrigger, SampleContext> Runner(
        StateMachineDefinition<SampleState, SampleTrigger, SampleContext> definition) =>
        new(definition, new SampleStateResolver(), new ActivatorStepFactory());
}

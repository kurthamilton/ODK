using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Workflows.Tests.Fakes;

namespace ODK.Core.Workflows.Tests;

[Parallelizable]
public static class StateMachineBuilderTests
{
    [Test]
    public static void Build_ValidDefinition_ReturnsDefinition()
    {
        // Arrange
        var builder = Valid();

        // Act
        var result = builder.Build();

        // Assert
        result.Name.Should().Be("Sample");
        result.InitialState.Should().Be(SampleState.Start);
        result.Transitions.Should().HaveCount(2);
    }

    [Test]
    public static void Build_StepDeclaredOnTransition_TakesMetadataFromTheStepType()
    {
        // Arrange
        var builder = Valid(x => x.Then<WriteStep>());

        // Act
        var result = builder.Build();

        // Assert
        var step = result.Transitions.First().Steps.Single();
        step.Description.Should().Be(WriteStep.Description);
        step.Kind.Should().Be(StepKind.Write);
        step.StepType.Should().Be<WriteStep>();
    }

    [Test]
    public static void Build_NoInitialState_Throws()
    {
        // Arrange
        var builder = StateMachine
            .Define<SampleState, SampleTrigger, SampleContext>("Sample")
            .Transition(SampleState.Start, SampleTrigger.Go, SampleState.Middle)
            .Transition(SampleState.Middle, SampleTrigger.Stop, SampleState.End);

        // Act
        var act = () => builder.Build();

        // Assert
        act.Should().Throw<StateMachineDefinitionException>().WithMessage("*no initial state*");
    }

    [Test]
    public static void Build_StateNeverWired_Throws()
    {
        // Arrange
        var builder = StateMachine
            .Define<SampleState, SampleTrigger, SampleContext>("Sample")
            .StartingAt(SampleState.Start)
            .Transition(SampleState.Start, SampleTrigger.Go, SampleState.Middle)
            .Transition(SampleState.Middle, SampleTrigger.Stop, SampleState.Start);

        // Act
        var act = () => builder.Build();

        // Assert
        act.Should().Throw<StateMachineDefinitionException>()
            .WithMessage("*State End is declared but never entered or left*");
    }

    [Test]
    public static void Build_TriggerNeverFired_Throws()
    {
        // Arrange
        var builder = StateMachine
            .Define<SampleState, SampleTrigger, SampleContext>("Sample")
            .StartingAt(SampleState.Start)
            .Transition(SampleState.Start, SampleTrigger.Go, SampleState.Middle)
            .Transition(SampleState.Middle, SampleTrigger.Go, SampleState.End);

        // Act
        var act = () => builder.Build();

        // Assert
        act.Should().Throw<StateMachineDefinitionException>()
            .WithMessage("*Trigger Stop is declared but never fired*");
    }

    [Test]
    public static void Build_GuardWithoutDescription_Throws()
    {
        // Arrange
        var builder = Valid(x => x.When(new SampleGuard(string.Empty, true)));

        // Act
        var act = () => builder.Build();

        // Assert
        act.Should().Throw<StateMachineDefinitionException>()
            .WithMessage("*SampleGuard has no description*");
    }

    [Test]
    public static void Build_StepWithoutDescription_Throws()
    {
        // Arrange
        var builder = Valid(x => x.Then<UndescribedStep>());

        // Act
        var act = () => builder.Build();

        // Assert
        act.Should().Throw<StateMachineDefinitionException>()
            .WithMessage("*UndescribedStep has no description*");
    }

    [Test]
    public static void Build_StepWithoutKind_Throws()
    {
        // Arrange
        var builder = Valid(x => x.Then<UnkindStep>());

        // Act
        var act = () => builder.Build();

        // Assert
        act.Should().Throw<StateMachineDefinitionException>()
            .WithMessage("*UnkindStep does not say what kind of step it is*");
    }

    [Test]
    public static void Build_ExternalEffectWhileAWriteIsUncommitted_Throws()
    {
        // Arrange
        var builder = Valid(x => x
            .Then<WriteStep>()
            .Then<ExternalEffectStep>());

        // Act
        var act = () => builder.Build();

        // Assert
        act.Should().Throw<StateMachineDefinitionException>()
            .WithMessage("*ExternalEffectStep takes an external effect while a write is uncommitted*");
    }

    [Test]
    public static void Build_ExternalEffectAfterACommit_ReturnsDefinition()
    {
        // Arrange
        var builder = Valid(x => x
            .Then<WriteStep>()
            .Then<CommitStep>()
            .Then<ExternalEffectStep>());

        // Act
        var result = builder.Build();

        // Assert
        result.Transitions.First().Steps.Should().HaveCount(3);
    }

    [Test]
    public static void Build_ExternalEffectWithNothingStaged_ReturnsDefinition()
    {
        // Arrange
        var builder = Valid(x => x
            .Then<DecisionStep>()
            .Then<ExternalEffectStep>());

        // Act
        var result = builder.Build();

        // Assert
        result.Transitions.First().Steps.Should().HaveCount(2);
    }

    [Test]
    public static void Build_TransitionDeclaredTwice_Throws()
    {
        // Arrange
        var builder = StateMachine
            .Define<SampleState, SampleTrigger, SampleContext>("Sample")
            .StartingAt(SampleState.Start)
            .Transition(SampleState.Start, SampleTrigger.Go, SampleState.Middle)
            .Transition(SampleState.Start, SampleTrigger.Go, SampleState.Middle)
            .Transition(SampleState.Middle, SampleTrigger.Stop, SampleState.End);

        // Act
        var act = () => builder.Build();

        // Assert
        act.Should().Throw<StateMachineDefinitionException>()
            .WithMessage("*is declared more than once*");
    }

    [Test]
    public static void Build_TransitionsDifferingOnlyByGuard_ReturnsDefinition()
    {
        // Arrange
        var builder = StateMachine
            .Define<SampleState, SampleTrigger, SampleContext>("Sample")
            .StartingAt(SampleState.Start)
            .Transition(
                SampleState.Start,
                SampleTrigger.Go,
                SampleState.Middle,
                x => x.When(new SampleGuard("the flag is set", true)))
            .Transition(
                SampleState.Start,
                SampleTrigger.Go,
                SampleState.Middle,
                x => x.When(new SampleGuard("the flag is not set", false)))
            .Transition(SampleState.Middle, SampleTrigger.Stop, SampleState.End);

        // Act
        var result = builder.Build();

        // Assert
        result.Transitions.Should().HaveCount(3);
    }

    private static StateMachineBuilder<SampleState, SampleTrigger, SampleContext> Valid(
        Action<TransitionBuilder<SampleContext>>? configure = null) => StateMachine
        .Define<SampleState, SampleTrigger, SampleContext>("Sample")
        .StartingAt(SampleState.Start)
        .Transition(SampleState.Start, SampleTrigger.Go, SampleState.Middle, configure)
        .Transition(SampleState.Middle, SampleTrigger.Stop, SampleState.End);

    private sealed class UndescribedStep : IStep<SampleContext>
    {
        public static string Description => string.Empty;

        public static StepKind Kind => StepKind.Decision;

        public Task<StepOutcome> Execute(SampleContext context, CancellationToken cancellationToken) =>
            Task.FromResult(StepOutcome.Continue());
    }

    private sealed class UnkindStep : IStep<SampleContext>
    {
        public static string Description => "does something unclassified";

        public static StepKind Kind => StepKind.None;

        public Task<StepOutcome> Execute(SampleContext context, CancellationToken cancellationToken) =>
            Task.FromResult(StepOutcome.Continue());
    }
}

using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Workflows.Tests.Fakes;

namespace ODK.Core.Workflows.Tests;

[Parallelizable]
public static class MermaidExporterTests
{
    [Test]
    public static void ToStateDiagram_Definition_RendersEveryTransition()
    {
        // Arrange
        var definition = StateMachine
            .Define<SampleState, SampleTrigger, SampleContext>("Sample")
            .StartingAt(SampleState.Start)
            .Transition(SampleState.Start, SampleTrigger.Go, SampleState.Middle)
            .Transition(SampleState.Middle, SampleTrigger.Stop, SampleState.End)
            .Build();

        // Act
        var result = MermaidExporter.ToStateDiagram(definition);

        // Assert
        result.Should().Be(
            "stateDiagram-v2\n" +
            "    [*] --> Start\n" +
            "    Start --> Middle: Go\n" +
            "    Middle --> End: Stop\n");
    }

    [Test]
    public static void ToStateDiagram_GuardedTransition_LabelsTheEdgeWithTheGuards()
    {
        // Arrange
        var definition = StateMachine
            .Define<SampleState, SampleTrigger, SampleContext>("Sample")
            .StartingAt(SampleState.Start)
            .Transition(
                SampleState.Start,
                SampleTrigger.Go,
                SampleState.Middle,
                x => x.When(new FlagIsSet()))
            .Transition(
                SampleState.Start,
                SampleTrigger.Go,
                SampleState.End,
                x => x.When(Guard.Not(new FlagIsSet())))
            .Transition(SampleState.Middle, SampleTrigger.Stop, SampleState.End)
            .Build();

        // Act
        var result = MermaidExporter.ToStateDiagram(definition);

        // Assert
        result.Should().Contain("    Start --> Middle: Go [the flag is set]\n");
        result.Should().Contain("    Start --> End: Go [not the flag is set]\n");
    }

    [Test]
    public static void ToStateDiagram_DescribedTransition_LabelsTheEdgeWithTheDescription()
    {
        // Arrange
        var definition = StateMachine
            .Define<SampleState, SampleTrigger, SampleContext>("Sample")
            .StartingAt(SampleState.Start)
            .Transition(
                SampleState.Start,
                SampleTrigger.Go,
                SampleState.Middle,
                x => x.Describe("gets going").When(new FlagIsSet()))
            .Transition(SampleState.Middle, SampleTrigger.Stop, SampleState.End)
            .Build();

        // Act
        var result = MermaidExporter.ToStateDiagram(definition);

        // Assert
        result.Should().Contain("    Start --> Middle: gets going\n");
    }

    [Test]
    public static void ToStateDiagram_Definition_UsesLineFeedsSoOutputDoesNotVaryByMachine()
    {
        // Arrange
        var definition = StateMachine
            .Define<SampleState, SampleTrigger, SampleContext>("Sample")
            .StartingAt(SampleState.Start)
            .Transition(SampleState.Start, SampleTrigger.Go, SampleState.Middle)
            .Transition(SampleState.Middle, SampleTrigger.Stop, SampleState.End)
            .Build();

        // Act
        var result = MermaidExporter.ToStateDiagram(definition);

        // Assert
        result.Should().NotContain("\r");
    }
}

using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Workflows.Tests.Fakes;

namespace ODK.Core.Workflows.Tests;

[Parallelizable]
public static class MarkdownExporterTests
{
    [Test]
    public static void ToDocument_Definition_TitlesThePageAfterTheMachine()
    {
        // Arrange
        var definition = Definition();

        // Act
        var result = MarkdownExporter.ToDocument(definition);

        // Assert
        result.Should().StartWith("# Sample\n");
    }

    [Test]
    public static void ToDocument_Definition_EmbedsTheDiagram()
    {
        // Arrange
        var definition = Definition();

        // Act
        var result = MarkdownExporter.ToDocument(definition);

        // Assert
        result.Should().Contain("```mermaid\nstateDiagram-v2\n");
        result.Should().Contain("    Start --> Middle: Go [the flag is set]\n");
    }

    [Test]
    public static void ToDocument_TransitionWithSteps_ListsThemInOrderWithTheirKinds()
    {
        // Arrange
        var definition = Definition();

        // Act
        var result = MarkdownExporter.ToDocument(definition);

        // Assert
        result.Should().Contain(
            "| Start | Go | Middle | the flag is set | 1. writes something (Write)<br>2. commits (Commit) |");
    }

    [Test]
    public static void ToDocument_TransitionWithNoGuardsOrSteps_LeavesThoseCellsEmpty()
    {
        // Arrange
        var definition = Definition();

        // Act
        var result = MarkdownExporter.ToDocument(definition);

        // Assert
        result.Should().Contain("| Middle | Stop | End | - | - |");
    }

    private static StateMachineDefinition<SampleState, SampleTrigger, SampleContext> Definition() => StateMachine
        .Define<SampleState, SampleTrigger, SampleContext>("Sample")
        .StartingAt(SampleState.Start)
        .Transition(
            SampleState.Start,
            SampleTrigger.Go,
            SampleState.Middle,
            x => x
                .When(new FlagIsSet())
                .Then<WriteStep>()
                .Then<CommitStep>())
        .Transition(SampleState.Middle, SampleTrigger.Stop, SampleState.End)
        .Build();
}

using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Workflows;
using ODK.Services.Chapters.Workflows;

namespace ODK.Services.Tests.Chapters.Workflows;

[Parallelizable]
public static class ChapterPublicationStateMachineTests
{
    [Test]
    public static void Create_Definition_IsValid()
    {
        // Arrange
        var act = () => ChapterPublicationStateMachine.Create();

        // Act
        var result = act();

        // Assert
        act.Should().NotThrow();
        result.Transitions.Should().NotBeEmpty();
    }

    [Test]
    public static void Create_Definition_NeedsNoGuards()
    {
        /* Arrange - the state decides which move is legal here, so a condition would add nothing. Worth
           asserting: it is the evidence that the framework does not require guards to be useful. */
        var definition = ChapterPublicationStateMachine.Create();

        // Act
        var guards = definition.Transitions.SelectMany(x => x.Guards);

        // Assert
        guards.Should().BeEmpty();
    }

    [Test]
    public static void Create_Publish_IsOnlyLegalFromApproved()
    {
        // Arrange
        var definition = ChapterPublicationStateMachine.Create();

        // Act
        var result = definition.Transitions
            .Where(x => x.Trigger == ChapterPublicationTrigger.Publish)
            .Select(x => $"{x.From} -> {x.To}")
            .ToArray();

        // Assert
        result.Should().BeEquivalentTo("Approved -> Published");
    }

    [Test]
    public static void Create_ApproveWhenAlreadyApproved_IsLegalAndDoesNothing()
    {
        /* Arrange - approving an approved group is not a mistake, so the edge exists and carries no work,
           rather than being absent and reporting the trigger as illegal. */
        var definition = ChapterPublicationStateMachine.Create();

        // Act
        var idempotent = definition.Transitions
            .Where(x => x.Trigger == ChapterPublicationTrigger.Approve &&
                        x.From != ChapterPublicationState.Draft)
            .ToArray();

        // Assert
        idempotent.Should().HaveCount(2);
        idempotent.Should().OnlyContain(x => x.From == x.To);
        idempotent.SelectMany(x => x.Steps).Should().BeEmpty();
    }
}

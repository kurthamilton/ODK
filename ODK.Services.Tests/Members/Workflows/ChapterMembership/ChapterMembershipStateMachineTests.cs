using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Workflows;
using ODK.Services.Members.Workflows.ChapterMembership;

namespace ODK.Services.Tests.Members.Workflows.ChapterMembership;

[Parallelizable]
public static class ChapterMembershipStateMachineTests
{
    [Test]
    public static void Create_Definition_IsValid()
    {
        // Arrange
        var act = () => ChapterMembershipStateMachine.Create();

        // Act
        var result = act();

        // Assert
        act.Should().NotThrow();
        result.Transitions.Should().NotBeEmpty();
    }

    [Test]
    public static void Create_Join_CoversEveryStateAMemberCouldJoinFrom()
    {
        // Arrange
        var definition = ChapterMembershipStateMachine.Create();

        // Act
        var result = definition.Transitions
            .Where(x => x.Trigger == ChapterMembershipTrigger.Join)
            .Select(x => $"{x.From} -> {x.To}: {x.Label()}")
            .ToArray();

        // Assert
        result.Should().BeEquivalentTo(
            "NotJoined -> PendingApproval: Join [requiring approval]",
            "NotJoined -> Joined: Join [not requiring approval]",
            "Invited -> Joined: Join");
    }

    [Test]
    public static void Create_EveryJoinEdge_RunsTheSameSteps()
    {
        /* Arrange - the edges differ only in where they land, so a step list that drifted between them would
           mean joining did different work depending on approval. */
        var definition = ChapterMembershipStateMachine.Create();

        // Act
        var stepLists = definition.Transitions
            .Where(x => x.Trigger == ChapterMembershipTrigger.Join)
            .Select(x => string.Join(" | ", x.Steps.Select(step => step.Description)))
            .Distinct()
            .ToArray();

        // Assert
        stepLists.Should().HaveCount(1);
        stepLists.Single().Should().NotBeEmpty();
    }

    [Test]
    public static void Create_SignUp_StagesWritesAndNothingElse()
    {
        /* Arrange - on Drunken Knitwits signing up creates the account and joins the group in one transaction,
           which the account machine owns: its SignUp transition runs this machine's as a step, then commits and
           sends the email. So these edges must stage writes and stop there. A commit here would split the
           transaction in two, and an email here would be sent before it. The builder cannot check this, because
           it cannot see inside the step that runs one machine from another. */
        var definition = ChapterMembershipStateMachine.Create();

        // Act
        var kinds = definition.Transitions
            .Where(x => x.Trigger == ChapterMembershipTrigger.SignUp)
            .SelectMany(x => x.Steps)
            .Select(x => x.Kind)
            .Distinct()
            .ToArray();

        // Assert
        kinds.Should().OnlyContain(x => x == StepKind.Write || x == StepKind.Decision);
    }

    [Test]
    public static void Create_Invite_StagesWritesAndNothingElse()
    {
        /* Arrange - an import is a batch: the caller commits the whole file once and enqueues the emails after.
           A commit here would commit per row, and an email here would be sent before the file was saved. */
        var definition = ChapterMembershipStateMachine.Create();

        // Act
        var kinds = definition.Transitions
            .Where(x => x.Trigger == ChapterMembershipTrigger.Invite)
            .SelectMany(x => x.Steps)
            .Select(x => x.Kind)
            .Distinct()
            .ToArray();

        // Assert
        kinds.Should().OnlyContain(x => x == StepKind.Write || x == StepKind.Decision);
    }

    [Test]
    public static void Create_Definition_KnowsNothingAboutSigningIn()
    {
        /* Arrange - what a member is to a group does not depend on whether their account can sign in, so no
           edge here turns on activation. */
        var definition = ChapterMembershipStateMachine.Create();

        // Act
        var guards = definition.Transitions
            .SelectMany(x => x.Guards)
            .Select(x => x.Description);

        // Assert
        guards.Should().OnlyContain(x => !x.Contains("activat", System.StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public static void Create_Approve_RecordsThenCommitsThenTellsTheMember()
    {
        /* Arrange - the email must follow the commit: an approval an admin has been told about, then rolled
           back, cannot be taken back out of the member's inbox. */
        var definition = ChapterMembershipStateMachine.Create();

        // Act
        var kinds = definition.Transitions
            .Single(x => x.Trigger == ChapterMembershipTrigger.Approve &&
                         x.From == ChapterMembershipState.PendingApproval)
            .Steps
            .Select(x => x.Kind)
            .ToArray();

        // Assert
        kinds.Should().Equal(StepKind.Write, StepKind.Commit, StepKind.ExternalEffect);
    }

    [Test]
    public static void Create_ApproveWhenAlreadyApproved_IsLegalAndDoesNothing()
    {
        /* Arrange - approving a member who is already in is not a mistake, so the edge exists and carries no
           work rather than being absent and reporting the trigger as illegal. Its emptiness is what stops a
           second click sending a second approval email. */
        var definition = ChapterMembershipStateMachine.Create();

        // Act
        var idempotent = definition.Transitions
            .Where(x => x.Trigger == ChapterMembershipTrigger.Approve &&
                        x.From != ChapterMembershipState.PendingApproval)
            .ToArray();

        // Assert
        idempotent.Should().ContainSingle();
        idempotent.Should().OnlyContain(x => x.From == x.To);
        idempotent.SelectMany(x => x.Steps).Should().BeEmpty();
    }
}

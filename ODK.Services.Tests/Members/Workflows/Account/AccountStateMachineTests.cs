using System;
using System.Linq;
using FluentAssertions;
using ODK.Core.Workflows;
using NUnit.Framework;
using ODK.Services.Members.Workflows.Account;
using ODK.Services.Members.Workflows.Account.Steps;

namespace ODK.Services.Tests.Members.Workflows.Account;

[Parallelizable]
public static class AccountStateMachineTests
{
    [Test]
    public static void Create_Definition_IsValid()
    {
        // Arrange
        var act = () => AccountStateMachine.Create();

        // Act
        var result = act();

        // Assert
        act.Should().NotThrow();
        result.Transitions.Should().NotBeEmpty();
    }

    [Test]
    public static void Create_SignUpWhileAnonymous_SeparatesTheThreeRoutesIn()
    {
        // Arrange
        var definition = AccountStateMachine.Create();

        // Act
        var result = definition
            .From(AccountState.Anonymous)
            .Where(x => x.Trigger == AccountTrigger.SignUp)
            .Select(x => $"{x.Label()} -> {x.To}")
            .ToArray();

        // Assert
        result.Should().BeEquivalentTo(
            "SignUp [to a group] -> Registered",
            "SignUp [not to a group, not verified by OAuth] -> Registered",
            "SignUp [not to a group, verified by OAuth] -> Activated");
    }

    [Test]
    public static void Create_Import_StagesWritesAndNothingElse()
    {
        /* Arrange - an import is a batch: the caller commits the whole file once and enqueues the emails after.
           A commit here would commit per row, and an email here would be sent before the file was saved. */
        var definition = AccountStateMachine.Create();

        // Act
        var kinds = definition.Transitions
            .Where(x => x.Trigger == AccountTrigger.Import)
            .SelectMany(x => x.Steps)
            .Select(x => x.Kind)
            .Distinct()
            .ToArray();

        // Assert
        kinds.Should().NotBeEmpty();
        kinds.Should().OnlyContain(x => x == StepKind.Write || x == StepKind.Decision);
    }

    [Test]
    public static void Create_Definition_KnowsNothingAboutGroups()
    {
        /* Arrange - the account machine is site-level. A state named for a group would mean the two lifecycles
           had been folded back together, which is what the split undid. */
        var definition = AccountStateMachine.Create();

        // Act
        var states = definition.Transitions
            .SelectMany(x => new[] { x.From.ToString(), x.To.ToString() })
            .Distinct();

        // Assert
        states.Should().OnlyContain(x => !x.Contains("Group") && !x.Contains("Member"));
    }

    [Test]
    public static void Create_Activate_CoversTheGroupAndSiteRoutesAndNothingElse()
    {
        /* Arrange - following a link either happened inside a group or on the site, and the absent chapter is
           what tells them apart. A third edge would mean a route nobody can arrive by. */
        var definition = AccountStateMachine.Create();

        // Act
        var result = definition.Transitions
            .Where(x => x.Trigger == AccountTrigger.Activate)
            .Select(x => $"{x.From} -> {x.To}: {x.Label()}")
            .ToArray();

        // Assert
        result.Should().BeEquivalentTo(
            "Registered -> Activated: Activate [in a group]",
            "Registered -> Activated: Activate [not in a group]");
    }

    [Test]
    public static void Create_EveryActivateEdge_ChecksThePasswordBeforeWritingAnything()
    {
        /* Arrange - a refused password has to leave the account exactly as it was, still awaiting activation.
           That holds only while the check is the first step, so assert the position rather than presence. */
        var definition = AccountStateMachine.Create();

        // Act
        var firstKinds = definition.Transitions
            .Where(x => x.Trigger == AccountTrigger.Activate)
            .Select(x => x.Steps.First().Kind)
            .ToArray();

        // Assert
        firstKinds.Should().HaveCount(2);
        firstKinds.Should().OnlyContain(x => x == StepKind.Decision);
    }

    [Test]
    public static void Create_AcceptInvite_OnlyLeavesAnAccountThatCannotSignIn()
    {
        /* Arrange - an invitation is accepted by giving the account an import raised its first password, so the
           only state it can be fired from is the one that has no password. An account that can already sign in
           accepts by signing in and using the group's join page, which is another machine's business. */
        var definition = AccountStateMachine.Create();

        // Act
        var result = definition.Transitions
            .Where(x => x.Trigger == AccountTrigger.AcceptInvite)
            .Select(x => $"{x.From} -> {x.To}: {x.Label()}")
            .ToArray();

        // Assert
        result.Should().BeEquivalentTo("Registered -> Activated: AcceptInvite");
    }

    [Test]
    public static void Create_AcceptInvite_ChecksThePasswordFirstAndJoinsBeforeTheCommit()
    {
        /* Arrange - the same two rules the activate edges follow, for the same reasons: a refused password has
           to leave the account exactly as it was, and the membership has to be in the same commit as the
           activation, because an account activated without it would have accepted nothing. */
        var definition = AccountStateMachine.Create();

        // Act
        var steps = definition.Transitions
            .Single(x => x.Trigger == AccountTrigger.AcceptInvite)
            .Steps
            .ToArray();

        // Assert
        steps.First().Kind.Should().Be(StepKind.Decision);

        var commit = Array.FindIndex(steps, x => x.Kind == StepKind.Commit);
        commit.Should().BeGreaterThan(0);

        var joins = Array.FindIndex(steps, x => x.StepType == typeof(AcceptTheInvitation));
        joins.Should().BeInRange(1, commit - 1);

        // And the group is only told once the membership it is told about is durable.
        steps.Skip(commit + 1).Should().OnlyContain(x => x.Kind == StepKind.ExternalEffect);
    }

    [Test]
    public static void Create_EveryActivateEdge_TellsSomebodyOnlyAfterTheCommit()
    {
        /* Arrange - both edges end by sending mail, and an email announcing an activation that then rolled
           back cannot be taken out of an inbox. The builder enforces this, so the test is here to say that
           the rule is the point of the ordering rather than an accident of it. */
        var definition = AccountStateMachine.Create();

        // Act
        var tails = definition.Transitions
            .Where(x => x.Trigger == AccountTrigger.Activate)
            .Select(x => x.Steps.SkipWhile(step => step.Kind != StepKind.Commit).Skip(1).ToArray())
            .ToArray();

        // Assert
        tails.Should().HaveCount(2);
        tails.Should().OnlyContain(steps => steps.Length > 0);
        tails.SelectMany(x => x).Should().OnlyContain(step => step.Kind == StepKind.ExternalEffect);
    }
}

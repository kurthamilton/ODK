using System.Linq;
using FluentAssertions;
using ODK.Core.Workflows;
using NUnit.Framework;
using ODK.Services.Members.Workflows.Account;

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
}

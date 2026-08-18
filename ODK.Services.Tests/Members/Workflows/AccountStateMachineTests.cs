using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Platforms;
using ODK.Services.Members.Workflows;

namespace ODK.Services.Tests.Members.Workflows;

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
    public static void Create_Definition_ReachesEveryStateFromTheInitialState()
    {
        // Arrange
        var definition = AccountStateMachine.Create();

        // Act
        var reached = new HashSet<AccountState> { definition.InitialState };
        var queue = new Queue<AccountState>([definition.InitialState]);
        while (queue.Count > 0)
        {
            foreach (var transition in definition.From(queue.Dequeue()).Where(x => reached.Add(x.To)))
            {
                queue.Enqueue(transition.To);
            }
        }

        // Assert
        var declared = Enum.GetValues<AccountState>().Where(x => x != AccountState.None);
        reached.Should().BeEquivalentTo(declared);
    }

    [Test]
    public static void Create_SignUpWhileInvited_CoversBothPlatforms()
    {
        // Arrange
        var definition = AccountStateMachine.Create();

        // Act
        var result = definition
            .From(AccountState.Invited)
            .Where(x => x.Trigger == AccountTrigger.SignUp)
            .Select(x => x.Label())
            .ToArray();

        // Assert
        result.Should().BeEquivalentTo(
            $"SignUp [on {PlatformType.DrunkenKnitwits}, presented with the invitation token]",
            $"SignUp [on {PlatformType.DrunkenKnitwits}, not presented with the invitation token]",
            $"SignUp [on {PlatformType.Default}]");
    }
}

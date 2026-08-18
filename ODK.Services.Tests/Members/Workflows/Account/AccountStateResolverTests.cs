using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Services.Members.Workflows.Account;

namespace ODK.Services.Tests.Members.Workflows.Account;

[Parallelizable]
public static class AccountStateResolverTests
{
    [Test]
    public static void Resolve_NoAccount_ReturnsAnonymous()
    {
        // Arrange
        var context = Context(member: null);

        // Act
        var result = new AccountStateResolver().Resolve(context);

        // Assert
        result.Should().Be(AccountState.Anonymous);
    }

    [Test]
    public static void Resolve_UnactivatedAccount_ReturnsRegistered()
    {
        // Arrange
        var context = Context(new Member { Activated = false, Id = Guid.NewGuid() });

        // Act
        var result = new AccountStateResolver().Resolve(context);

        // Assert
        result.Should().Be(AccountState.Registered);
    }

    [Test]
    public static void Resolve_ActivatedAccount_ReturnsActivated()
    {
        // Arrange
        var context = Context(new Member { Activated = true, Id = Guid.NewGuid() });

        // Act
        var result = new AccountStateResolver().Resolve(context);

        // Assert
        result.Should().Be(AccountState.Activated);
    }

    [Test]
    public static void Resolve_EveryCombinationOfTheDomainItReads_ReturnsOneDeclaredState()
    {
        /* Arrange - derived state has to be total: nothing stores it, so every combination the domain can be in
           has to land on exactly one state. The account machine reads only whether an account exists and
           whether it is activated; what the member is to a group is a separate machine. */
        var resolver = new AccountStateResolver();
        var contexts = new List<AccountContext>
        {
            Context(member: null),
            Context(new Member { Activated = false, Id = Guid.NewGuid() }),
            Context(new Member { Activated = true, Id = Guid.NewGuid() })
        };

        // Act
        var results = contexts.Select(resolver.Resolve).ToArray();

        // Assert
        results.Should().OnlyContain(x => x != AccountState.None);
        results.Should().OnlyHaveUniqueItems();
    }

    private static AccountContext Context(Member? member) => new()
    {
        Member = member,
        Request = Mock.Of<IServiceRequest>(),
        VerifiedByOAuth = false
    };
}

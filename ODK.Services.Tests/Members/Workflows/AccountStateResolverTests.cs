using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Members.Workflows;

namespace ODK.Services.Tests.Members.Workflows;

[Parallelizable]
public static class AccountStateResolverTests
{
    private static readonly Guid ChapterId = Guid.NewGuid();

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
    public static void Resolve_UnactivatedAccountHoldingAnInvitation_ReturnsInvited()
    {
        // Arrange
        var context = Context(Member(activated: false), invited: true);

        // Act
        var result = new AccountStateResolver().Resolve(context);

        // Assert
        result.Should().Be(AccountState.Invited);
    }

    [Test]
    public static void Resolve_UnactivatedAccountWithNoInvitation_ReturnsRegistered()
    {
        // Arrange
        var context = Context(Member(activated: false));

        // Act
        var result = new AccountStateResolver().Resolve(context);

        // Assert
        result.Should().Be(AccountState.Registered);
    }

    [Test]
    public static void Resolve_UnactivatedAccountAlreadyInTheGroup_ReturnsRegistered()
    {
        /* Arrange - the state a Drunken Knitwits sign-up leaves behind: the membership is written before the
           account can sign in. Being unable to sign in has to outrank the membership, or a member who cannot
           act would be reported as a member of the group. */
        var context = Context(Member(activated: false, membership: true, approved: true));

        // Act
        var result = new AccountStateResolver().Resolve(context);

        // Assert
        result.Should().Be(AccountState.Registered);
    }

    [Test]
    public static void Resolve_ActivatedAccountInNoGroup_ReturnsActivated()
    {
        // Arrange
        var context = Context(Member(activated: true));

        // Act
        var result = new AccountStateResolver().Resolve(context);

        // Assert
        result.Should().Be(AccountState.Activated);
    }

    [Test]
    public static void Resolve_ActivatedAccountAwaitingApproval_ReturnsPendingApproval()
    {
        // Arrange
        var context = Context(Member(activated: true, membership: true, approved: false));

        // Act
        var result = new AccountStateResolver().Resolve(context);

        // Assert
        result.Should().Be(AccountState.PendingApproval);
    }

    [Test]
    public static void Resolve_ActivatedApprovedMember_ReturnsGroupMember()
    {
        // Arrange
        var context = Context(Member(activated: true, membership: true, approved: true));

        // Act
        var result = new AccountStateResolver().Resolve(context);

        // Assert
        result.Should().Be(AccountState.GroupMember);
    }

    [Test]
    public static void Resolve_EveryCombinationOfTheDomainItReads_ReturnsOneDeclaredState()
    {
        /* Arrange - derived state has to be total: nothing stores it, so every combination the domain can be
           in has to land on exactly one state. This is what stops a member falling through the machine. */
        var resolver = new AccountStateResolver();
        var contexts = Combinations().ToArray();

        // Act
        var results = contexts.Select(resolver.Resolve).ToArray();

        // Assert
        // Two invitation states x (no account + two activation states x three memberships).
        contexts.Should().HaveCount(14);
        results.Should().OnlyContain(x => x != AccountState.None);
    }

    private static IEnumerable<AccountContext> Combinations()
    {
        foreach (var invited in new[] { false, true })
        {
            yield return Context(member: null, invited);

            foreach (var activated in new[] { false, true })
            {
                yield return Context(Member(activated), invited);
                yield return Context(Member(activated, membership: true, approved: false), invited);
                yield return Context(Member(activated, membership: true, approved: true), invited);
            }
        }
    }

    private static AccountContext Context(Member? member, bool invited = false) => AccountContexts.Create(
        ChapterId,
        member,
        invite: invited
            ? new MemberChapterInvite { ChapterId = ChapterId, MemberId = member?.Id ?? Guid.Empty }
            : null);

    private static Member Member(bool activated, bool membership = false, bool approved = false) => new()
    {
        Activated = activated,
        Chapters = membership
            ? [new MemberChapter { Approved = approved, ChapterId = ChapterId }]
            : [],
        Id = Guid.NewGuid()
    };
}

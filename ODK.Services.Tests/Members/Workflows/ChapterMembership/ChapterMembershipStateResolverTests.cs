using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Services.Members.Workflows.ChapterMembership;

namespace ODK.Services.Tests.Members.Workflows.ChapterMembership;

[Parallelizable]
public static class ChapterMembershipStateResolverTests
{
    private static readonly Guid ChapterId = Guid.NewGuid();

    [Test]
    public static void Resolve_NothingConnectsThemToTheGroup_ReturnsNotJoined()
    {
        // Arrange
        var context = Context(Member());

        // Act
        var result = new ChapterMembershipStateResolver().Resolve(context);

        // Assert
        result.Should().Be(ChapterMembershipState.NotJoined);
    }

    [Test]
    public static void Resolve_OutstandingInvitation_ReturnsInvited()
    {
        // Arrange
        var context = Context(Member(), invited: true);

        // Act
        var result = new ChapterMembershipStateResolver().Resolve(context);

        // Assert
        result.Should().Be(ChapterMembershipState.Invited);
    }

    [Test]
    public static void Resolve_UnapprovedMembership_ReturnsPendingApproval()
    {
        // Arrange
        var context = Context(Member(membership: true, approved: false));

        // Act
        var result = new ChapterMembershipStateResolver().Resolve(context);

        // Assert
        result.Should().Be(ChapterMembershipState.PendingApproval);
    }

    [Test]
    public static void Resolve_ApprovedMembership_ReturnsJoined()
    {
        // Arrange
        var context = Context(Member(membership: true, approved: true));

        // Act
        var result = new ChapterMembershipStateResolver().Resolve(context);

        // Assert
        result.Should().Be(ChapterMembershipState.Joined);
    }

    [Test]
    public static void Resolve_UnactivatedAccountHoldingAMembership_ReturnsJoined()
    {
        /* Arrange - the state a Drunken Knitwits sign-up leaves behind: the membership is written before the
           account can sign in. What the member is to the group does not depend on that, which is the whole
           reason the two lifecycles are separate machines. */
        var context = Context(Member(activated: false, membership: true, approved: true));

        // Act
        var result = new ChapterMembershipStateResolver().Resolve(context);

        // Assert
        result.Should().Be(ChapterMembershipState.Joined);
    }

    [Test]
    public static void Resolve_EveryCombinationOfTheDomainItReads_ReturnsOneDeclaredState()
    {
        /* Arrange - derived state has to be total: nothing stores it, so every combination the domain can be in
           has to land on exactly one state. */
        var resolver = new ChapterMembershipStateResolver();
        var contexts = Combinations().ToArray();

        // Act
        var results = contexts.Select(resolver.Resolve).ToArray();

        // Assert
        // Two invitation states x three memberships (none, unapproved, approved).
        contexts.Should().HaveCount(6);
        results.Should().OnlyContain(x => x != ChapterMembershipState.None);
    }

    private static IEnumerable<ChapterMembershipContext> Combinations()
    {
        foreach (var invited in new[] { false, true })
        {
            yield return Context(Member(), invited);
            yield return Context(Member(membership: true, approved: false), invited);
            yield return Context(Member(membership: true, approved: true), invited);
        }
    }

    private static ChapterMembershipContext Context(Member member, bool invited = false) =>
        ChapterMembershipContexts.Create(
            ChapterId,
            member,
            invite: invited
                ? new MemberChapterInvite { ChapterId = ChapterId, MemberId = member.Id }
                : null);

    private static Member Member(bool activated = true, bool membership = false, bool approved = false) => new()
    {
        Activated = activated,
        Chapters = membership
            ? [new MemberChapter { Approved = approved, ChapterId = ChapterId }]
            : [],
        Id = Guid.NewGuid()
    };
}

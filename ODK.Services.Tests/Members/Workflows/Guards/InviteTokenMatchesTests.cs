using System;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Members.Workflows;
using ODK.Services.Members.Workflows.Guards;

namespace ODK.Services.Tests.Members.Workflows.Guards;

[Parallelizable]
public static class InviteTokenMatchesTests
{
    [Test]
    public static void IsSatisfied_TokenMatchesTheInvitation_ReturnsTrue()
    {
        // Arrange
        var guard = new InviteTokenMatches();
        var context = Context(Invite("token"), "token");

        // Act
        var result = guard.IsSatisfied(context);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void IsSatisfied_TokenIsForAnotherInvitation_ReturnsFalse()
    {
        // Arrange
        var guard = new InviteTokenMatches();
        var context = Context(Invite("token"), "another token");

        // Act
        var result = guard.IsSatisfied(context);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void IsSatisfied_NoInvitation_ReturnsFalse()
    {
        // Arrange
        var guard = new InviteTokenMatches();
        var context = Context(invite: null, "token");

        // Act
        var result = guard.IsSatisfied(context);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void IsSatisfied_NoTokenPresented_ReturnsFalse()
    {
        // Arrange
        var guard = new InviteTokenMatches();
        var context = Context(Invite("token"), inviteToken: null);

        // Act
        var result = guard.IsSatisfied(context);

        // Assert
        result.Should().BeFalse();
    }

    private static AccountContext Context(MemberChapterInvite? invite, string? inviteToken) =>
        AccountContexts.Create(Guid.NewGuid(), invite: invite, inviteToken: inviteToken);

    private static MemberChapterInvite Invite(string token) => new()
    {
        ChapterId = Guid.NewGuid(),
        CreatedUtc = DateTime.UtcNow,
        MemberId = Guid.NewGuid(),
        Token = token
    };
}

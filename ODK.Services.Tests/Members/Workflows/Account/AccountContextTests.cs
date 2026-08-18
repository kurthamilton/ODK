using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Services;
using ODK.Services.Members.Workflows.Account;

namespace ODK.Services.Tests.Members.Workflows.Account;

[Parallelizable]
public static class AccountContextTests
{
    [Test]
    public static void NewMember_SetOnce_IsTheAccountTheStepCreated()
    {
        // Arrange
        var context = Context();
        var member = new Member { Id = Guid.NewGuid() };

        // Act
        context.NewMember = member;

        // Assert
        context.NewMember.Should().BeSameAs(member);
        context.RequiredNewMember.Should().BeSameAs(member);
    }

    [Test]
    public static void NewMember_SetTwice_Throws()
    {
        /* Arrange - no transition creates two accounts, so a second write means two create steps ended up on
           one edge. Failing there beats silently keeping whichever ran last. */
        var context = Context();
        context.NewMember = new Member { Id = Guid.NewGuid() };

        // Act
        var act = () => context.NewMember = new Member { Id = Guid.NewGuid() };

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("The account the transition creates has already been set");
    }

    [Test]
    public static void RequiredNewMember_BeforeTheAccountIsCreated_Throws()
    {
        // Arrange
        var context = Context();

        // Act
        var act = () => context.RequiredNewMember;

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*before the one that creates*");
    }

    private static AccountContext Context() => new()
    {
        Request = Mock.Of<IServiceRequest>(),
        VerifiedByOAuth = false
    };
}

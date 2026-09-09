using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Services;
using ODK.Services.Members.Workflows.Account;
using ODK.Services.Members.Workflows.Account.Steps;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Members.Workflows.Account;

[Parallelizable]
public static class CarryOverInvitesTests
{
    [Test]
    public static async Task Execute_KeepsTheClockTheInviteAlreadyHad()
    {
        /* Arrange - the retention period runs from when the details were received, so re-raising an invite
           under a new account must not restart it. Stamping UtcNow would let a repeated signup hold an
           address indefinitely. */
        using var context = new MockOdkContext();
        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        var receivedUtc = DateTime.UtcNow.AddDays(-30);
        var chapterId = Guid.NewGuid();

        var accountContext = Context(new MemberChapterInvite
        {
            ChapterId = chapterId,
            CreatedUtc = receivedUtc,
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            SentUtc = receivedUtc,
            Token = "token"
        });

        // Act
        await new CarryOverInvites(unitOfWork).Execute(accountContext, CancellationToken.None);
        await unitOfWork.SaveChanges();

        // Assert
        var carried = context.Set<MemberChapterInvite>()
            .Should().ContainSingle(x => x.ChapterId == chapterId).Subject;

        carried.CreatedUtc.Should().BeCloseTo(receivedUtc, TimeSpan.FromSeconds(1));
        carried.MemberId.Should().Be(accountContext.RequiredNewMember.Id);

        // The link already emailed still works, and a group holding it still knows it has been sent.
        carried.Token.Should().Be("token");
        carried.SentUtc.Should().NotBeNull();
    }

    private static AccountContext Context(params MemberChapterInvite[] carriedOverInvites)
    {
        var context = new AccountContext
        {
            CarriedOverInvites = carriedOverInvites,
            Request = Mock.Of<IServiceRequest>(),
            VerifiedByOAuth = false
        };

        context.NewMember = new Member { Id = Guid.NewGuid() };

        return context;
    }
}

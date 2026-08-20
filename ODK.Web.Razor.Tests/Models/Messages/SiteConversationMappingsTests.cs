using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Messages;
using ODK.Data.Core.Messages;
using ODK.Web.Razor.Models.Messages;

namespace ODK.Web.Razor.Tests.Models.Messages;

[Parallelizable]
public static class SiteConversationMappingsTests
{
    private const string PlatformName = "PLATFORM";

    [Test]
    public static void ToMemberViewModels_MessageFromASiteAdmin_IsAttributedToThePlatform()
    {
        // Arrange
        var conversation = CreateConversation();
        var message = CreateMessage(Guid.NewGuid(), "Ada", "Admin");

        // Act
        var result = new[] { message }.ToMemberViewModels(conversation, PlatformName);

        // Assert
        result.Single().MemberFullName.Should().Be(PlatformName);
    }

    [Test]
    public static void ToMemberViewModels_MessageFromASiteAdmin_KeepsTheirMemberId()
    {
        // Arrange - the thread aligns each message and labels the member's own as "You" from the id, so
        // masking the name must not touch it.
        var conversation = CreateConversation();
        var adminId = Guid.NewGuid();
        var message = CreateMessage(adminId, "Ada", "Admin");

        // Act
        var result = new[] { message }.ToMemberViewModels(conversation, PlatformName);

        // Assert
        result.Single().MemberId.Should().Be(adminId);
    }

    [Test]
    public static void ToMemberViewModels_MessageFromTheMember_KeepsTheirName()
    {
        // Arrange
        var conversation = CreateConversation();
        var message = CreateMessage(conversation.MemberId, "Member", "Name");

        // Act
        var result = new[] { message }.ToMemberViewModels(conversation, PlatformName);

        // Assert
        result.Single().MemberFullName.Should().Be("Member Name");
    }

    [Test]
    public static void ToSiteAdminViewModels_MessageFromASiteAdmin_KeepsTheirName()
    {
        // Arrange - the admin area is the one place the sender is named: its audience is the admins.
        var message = CreateMessage(Guid.NewGuid(), "Ada", "Admin");

        // Act
        var result = new[] { message }.ToSiteAdminViewModels();

        // Assert
        result.Single().MemberFullName.Should().Be("Ada Admin");
    }

    private static SiteConversation CreateConversation() => new()
    {
        Id = Guid.NewGuid(),
        MemberId = Guid.NewGuid(),
        Subject = "SUBJECT"
    };

    private static SiteConversationMessageDto CreateMessage(Guid memberId, string firstName, string lastName)
        => new()
        {
            MemberFirstName = firstName,
            MemberLastName = lastName,
            Message = new SiteConversationMessage
            {
                Id = Guid.NewGuid(),
                MemberId = memberId,
                Text = "TEXT"
            }
        };
}

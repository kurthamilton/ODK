using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using ODK.Core.Workflows;
using ODK.Services.Geolocation;
using ODK.Services.Members.Workflows.Account;
using ODK.Services.Members.Workflows.ChapterMembership;
using ODK.Services.Notifications;
using ODK.Services.Recaptcha;
using ODK.Services.Topics;
using ODK.Services.Workflows;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Logging;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Emails;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Security;
using ODK.Services.Subscriptions;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Members;

[Parallelizable]
public static class MemberAdminServiceTests
{
    [Test]
    public static async Task ApproveMember_WhenMemberExists_ApprovesMemberAndTellsThem()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, member) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: currentMember,
            unapprovedMembers: [member]);

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateMemberAdminService(context, memberEmailService: emailService.Object);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberApprovals);

        // Act
        var result = await service.ApproveMember(request, member.Id);

        // Assert
        result.Success.Should().BeTrue();
        member = context.Set<Member>().Single(x => x.Id == member.Id);

        var memberChapter = member.MemberChapter(chapter.Id);
        memberChapter.Should().NotBeNull();
        memberChapter.Approved.Should().BeTrue();

        emailService.Verify(
            x => x.SendMemberApprovedEmail(It.IsAny<IChapterServiceRequest>(), It.IsAny<Member>()),
            Times.Once);
    }

    [Test]
    public static async Task ApproveMember_AlreadyApproved_SucceedsWithoutTellingThemAgain()
    {
        /* Arrange - approving a member who is already in is not a mistake, so it reports success. The machine
           expresses that as an Approve edge out of Joined that carries no work, which is why nothing here has
           to check first, and why a second click sends no second email. */
        using var context = CreateMockOdkContext();

        var (currentMember, member) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: currentMember,
            members: [member]);

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateMemberAdminService(context, memberEmailService: emailService.Object);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberApprovals);

        // Act
        var result = await service.ApproveMember(request, member.Id);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Member>()
            .Single(x => x.Id == member.Id)
            .MemberChapter(chapter.Id)!.Approved.Should().BeTrue();

        emailService.Verify(
            x => x.SendMemberApprovedEmail(It.IsAny<IChapterServiceRequest>(), It.IsAny<Member>()),
            Times.Never);
    }

    [Test]
    public static async Task GetAdminMemberViewModel_WhenValid_ReturnsViewModel()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, targetMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: currentMember,
            adminMembers: [targetMember]);

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetAdminMemberViewModel(request, targetMember.Id);

        // Assert
        result.Should().NotBeNull();
        result.AdminMember.MemberId.Should().Be(targetMember.Id);
        result.ReadOnly.Should().BeFalse();
        result.CanEditRole.Should().BeTrue();
    }

    [Test]
    public static async Task GetAdminMemberViewModel_WhenOwner_IsReadOnly_ForNonOwner()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, targetMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: targetMember,
            adminMembers: [currentMember]);

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetAdminMemberViewModel(request, targetMember.Id);

        // Assert
        result.ReadOnly.Should().BeTrue();
        result.CanEditRole.Should().BeFalse();
    }

    [Test]
    public static async Task GetAdminMemberViewModel_WhenOwner_CannotEditRole()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            owner: currentMember);

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetAdminMemberViewModel(request, currentMember.Id);

        // Assert
        result.ReadOnly.Should().BeFalse();
        result.CanEditRole.Should().BeFalse();
    }

    [Test]
    public static async Task GetAdminMemberViewModel_WhenOtherAdminWithSameRole_CannotEditRole()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, targetMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            adminMembers: [currentMember, targetMember]);

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetAdminMemberViewModel(request, targetMember.Id);

        // Assert
        result.ReadOnly.Should().BeFalse();
        result.CanEditRole.Should().BeFalse();
    }

    [Test]
    public static async Task GetAdminMemberViewModel_CanEditOwnRole()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetAdminMemberViewModel(request, currentMember.Id);

        // Assert
        result.ReadOnly.Should().BeFalse();
        result.CanEditRole.Should().BeTrue();
    }

    [Test]
    public static async Task GetMembersViewModel_WhenValid_ReturnsViewModel()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var members = new[] { context.CreateMember(), context.CreateMember() };

        var chapter = context.CreateChapter(
            adminMembers: [currentMember],
            members: members);

        var membershipSettings = context.Create(CreateChapterMembershipSettings(chapter: chapter));

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Members);

        // Act
        var result = await service.GetMembersViewModel(request);

        // Assert
        result.Should().NotBeNull();
        result.Members.Should().HaveCount(2);
        result.Chapter.Should().Be(chapter);
    }

    [Test]
    public static async Task GetMemberViewModel_WhenValid_ReturnsViewModel()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, member) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            adminMembers: [currentMember],
            members: [member]);

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Members);

        // Act
        var result = await service.GetMemberViewModel(request, member.Id);

        // Assert
        result.Should().NotBeNull();
        result.Member.Id.Should().Be(member.Id);
        result.Chapter.Should().Be(chapter);
    }

    [Test]
    public static async Task GetMemberViewModel_MarksNotificationsAsRead()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, member) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            adminMembers: [currentMember],
            members: [member]);

        var notification = context.Create(CreateNotification(
            currentMember,
            type: NotificationType.NewMember,
            entityId: member.Id));

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Members);

        // Act
        await service.GetMemberViewModel(request, member.Id);

        // Assert
        notification = context.Set<Notification>().Single(x => x.Id == notification.Id);
        notification.ReadUtc.Should().NotBeNull();
    }

    [Test]
    public static async Task GetInvitedMembersViewModel_ReturnsOutstandingInvitationsOldestFirst()
    {
        /* Arrange - three invitations raised out of order, so passing cannot be an accident of insertion
           order. The oldest invitation is the one that has been waiting longest, which is what an admin
           chasing acceptances wants at the top. */
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(owner: currentMember);

        var now = DateTime.UtcNow;
        var middle = context.CreateMember(afterCreate: x => x.FirstName = "Middle");
        var oldest = context.CreateMember(afterCreate: x => x.FirstName = "Oldest");
        var newest = context.CreateMember(afterCreate: x => x.FirstName = "Newest");

        CreateInvite(context, chapter.Id, middle.Id, now.AddDays(-2));
        CreateInvite(context, chapter.Id, oldest.Id, now.AddDays(-5));
        CreateInvite(context, chapter.Id, newest.Id, now.AddDays(-1));

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberImport);

        // Act
        var result = await service.GetInvitedMembersViewModel(request);

        // Assert
        result.Invited.Select(x => x.Member.FirstName)
            .Should()
            .Equal("Oldest", "Middle", "Newest");
        result.Invited.Select(x => x.CreatedUtc)
            .Should()
            .BeInAscendingOrder();
    }

    [Test]
    public static async Task GetInvitedMembersViewModel_MemberInvitedToAnotherGroup_IsNotListed()
    {
        // Arrange - the page is one group's outstanding invitations, and an invitation is per group.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(owner: currentMember);
        var other = context.CreateChapter(owner: currentMember);

        var invited = context.CreateMember(afterCreate: x => x.FirstName = "Invited");
        var elsewhere = context.CreateMember(afterCreate: x => x.FirstName = "Elsewhere");

        CreateInvite(context, chapter.Id, invited.Id, DateTime.UtcNow);
        CreateInvite(context, other.Id, elsewhere.Id, DateTime.UtcNow);

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberImport);

        // Act
        var result = await service.GetInvitedMembersViewModel(request);

        // Assert
        result.Invited.Should().ContainSingle().Which.Member.Id.Should().Be(invited.Id);
    }

    [Test]
    public static async Task GetInvitedMembersViewModel_NobodyInvited_ReturnsEmpty()
    {
        // Arrange - the ordinary case for a group that has never run an import.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(owner: currentMember);

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberImport);

        // Act
        var result = await service.GetInvitedMembersViewModel(request);

        // Assert
        result.Invited.Should().BeEmpty();
        result.Chapter.Id.Should().Be(chapter.Id);
    }

    [Test]
    public static async Task GetMemberApprovalsViewModel_ReturnsPendingMembers()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, approvedMember, pendingMember) = (context.CreateMember(), context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: currentMember,
            members: [approvedMember],
            unapprovedMembers: [pendingMember]);

        context.Create(CreateChapterMembershipSettings(chapter: chapter));

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberApprovals);

        // Act
        var result = await service.GetMemberApprovalsViewModel(request);

        // Assert
        result.Should().NotBeNull();
        result.Pending.Should().HaveCount(1);
        result.Pending.First().Id.Should().Be(pendingMember.Id);
    }

    [Test]
    public static async Task RemoveMemberFromChapter_WhenActiveSubscription_ReturnsFails()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, member) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: currentMember,
            members: [member]);

        // The removal guard reads the member's current subscription from the log (source of truth).
        context.Create(new MemberSubscriptionRecord
        {
            ChapterId = chapter.Id,
            ExpiresUtc = DateTime.UtcNow.AddDays(10),
            Id = Guid.NewGuid(),
            IsCurrent = true,
            MemberId = member.Id,
            PurchasedUtc = DateTime.UtcNow,
            Type = SubscriptionType.Full
        });

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberApprovals);

        // Act
        var result = await service.RemoveMemberFromChapter(request, member.Id, "test reason");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("cannot remove");
    }

    [Test]
    public static async Task RemoveMemberFromChapter_WhenNoSubscription_ReturnsSuccess()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, member) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: currentMember,
            members: [member]);

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberApprovals);

        // Act
        var result = await service.RemoveMemberFromChapter(request, member.Id, "test reason");

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateMemberSubscription_WhenValid_UpdatesSuccessfully()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, member) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: currentMember,
            members: [member]);

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberAdmin);

        var expiryDate = DateTime.UtcNow.AddDays(30);
        var model = new MemberSubscriptionUpdateModel
        {
            Type = SubscriptionType.Full,
            ExpiryDate = expiryDate
        };

        // Act
        var result = await service.UpdateMemberSubscription(request, member.Id, model);

        // Assert
        result.Success.Should().BeTrue();

        var subscription = context.Set<MemberSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.ChapterId == chapter.Id && x.IsCurrent);
        subscription.ExpiresUtc.Should().Be(expiryDate);
    }

    [Test]
    public static async Task UpdateMemberSubscription_WhenInvalidType_ReturnsFails()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, member) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: currentMember,
            members: [member]);

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberAdmin);

        var model = new MemberSubscriptionUpdateModel
        {
            Type = SubscriptionType.None,
            ExpiryDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = await service.UpdateMemberSubscription(request, member.Id, model);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid type");
    }

    [Test]
    public static async Task UpdateMemberImage_WhenValid_UpdatesSuccessfully()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, member) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: currentMember,
            members: [member]);

        var memberImageService = CreateMockMemberImageService(isValid: true);

        var service = CreateMemberAdminService(
            context,
            memberImageService: memberImageService);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberImage);

        var model = new MemberImageUpdateModel { ImageData = [1, 2, 3] };

        // Act
        var result = await service.UpdateMemberImage(request, member.Id, model);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Picture updated");
    }

    [Test]
    public static async Task UpdateMemberImage_WhenInvalidImage_ReturnsFails()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, member) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: currentMember,
            members: [member]);

        var memberImageService = CreateMockMemberImageService(isValid: false);

        var service = CreateMemberAdminService(
            context,
            memberImageService: memberImageService);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberImage);

        var model = new MemberImageUpdateModel { ImageData = [1, 2, 3] };

        // Act
        var result = await service.UpdateMemberImage(request, member.Id, model);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public static async Task GetMemberCsv_ReturnsMembersInCsvFormat()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, member) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: currentMember,
            members: [member]);

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberExport);

        // Act
        var result = await service.GetMemberCsv(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Header + 1 member
        result.First().Should().Contain("ID");
        result.First().Should().Contain("FirstName");
    }

    [Test]
    public static async Task ImportMembers_WhenFileHasDuplicateEmails_CreatesSingleMember()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(owner: currentMember);

        // A default site subscription is required for the platform when new members are created.
        context.Create(new SiteSubscription
        {
            Id = Guid.NewGuid(),
            Name = "Default",
            Description = "",
            GroupLimit = 10,
            Enabled = true,
            Default = true,
            Platform = PlatformType.Default,
            SitePaymentSettingId = Guid.NewGuid()
        });

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberImport);

        // The same address twice (differing only in case) must collapse to a single member.
        var members = new[]
        {
            new MemberImportModel { EmailAddress = "new@example.com", FirstName = "New", LastName = "Member" },
            new MemberImportModel { EmailAddress = "NEW@example.com", FirstName = "Dupe", LastName = "Member" }
        };

        // Act
        var result = await service.ImportMembers(request, members);

        // Assert
        result.Success.Should().BeTrue();

        context.Set<Member>()
            .Count(x => x.EmailAddress == "new@example.com" || x.EmailAddress == "NEW@example.com")
            .Should()
            .Be(1);
    }

    [Test]
    public static async Task ImportMembers_NewMember_InvitesThemWithoutGivingThemMembership()
    {
        /* Arrange - an imported member has no membership status until they activate their account and join, so
           the import records the invitation and nothing else. Creating the membership here would make them a
           member of a group they have never responded to. */
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(owner: currentMember);

        context.Create(new SiteSubscription
        {
            Id = Guid.NewGuid(),
            Name = "Default",
            Description = "",
            GroupLimit = 10,
            Enabled = true,
            Default = true,
            Platform = PlatformType.Default,
            SitePaymentSettingId = Guid.NewGuid()
        });

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberImport);

        var members = new[]
        {
            new MemberImportModel { EmailAddress = "new@example.com", FirstName = "New", LastName = "Member" }
        };

        // Act
        var result = await service.ImportMembers(request, members);

        // Assert
        result.Success.Should().BeTrue();

        var member = context.Set<Member>().Single(x => x.EmailAddress == "new@example.com");

        context.Set<MemberChapterInvite>()
            .Count(x => x.MemberId == member.Id && x.ChapterId == chapter.Id)
            .Should()
            .Be(1);

        context.Set<MemberChapter>()
            .Any(x => x.MemberId == member.Id && x.ChapterId == chapter.Id)
            .Should()
            .BeFalse();

        // Nor a subscription: the trial starts when they join, not when the file was uploaded.
        context.Set<MemberSubscriptionRecord>()
            .Any(x => x.MemberId == member.Id && x.ChapterId == chapter.Id)
            .Should()
            .BeFalse();
    }

    [Test]
    public static async Task ImportMembers_AlreadyInvited_DoesNotInviteAgain()
    {
        /* Arrange - re-importing the same file is a normal thing to do, and the unique index on
           (chapter, member) would reject a second invitation rather than ignore it. */
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(owner: currentMember);
        var invited = context.CreateMember();
        invited.EmailAddress = "invited@example.com";

        context.Create(new MemberChapterInvite
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            MemberId = invited.Id
        });

        // Read whether or not a new member is created, so it has to be present even though this row is skipped.
        context.Create(new SiteSubscription
        {
            Id = Guid.NewGuid(),
            Name = "Default",
            Description = "",
            GroupLimit = 10,
            Enabled = true,
            Default = true,
            Platform = PlatformType.Default,
            SitePaymentSettingId = Guid.NewGuid()
        });

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberImport);

        var members = new[]
        {
            new MemberImportModel
            {
                EmailAddress = "invited@example.com", FirstName = "Invited", LastName = "Member"
            }
        };

        // Act
        var result = await service.ImportMembers(request, members);

        // Assert
        result.Success.Should().BeTrue();

        context.Set<MemberChapterInvite>()
            .Count(x => x.MemberId == invited.Id && x.ChapterId == chapter.Id)
            .Should()
            .Be(1);
    }

    [TestCase(PlatformType.Default)]
    [TestCase(PlatformType.DrunkenKnitwits)]
    public static async Task ImportMembers_NewMember_SendsTheInvitation(PlatformType platform)
    {
        /* Arrange - both platforms have a page an invitation's link lands on that a member with no password can
           use, so both send the invitation. An activation link would take a new member straight past the group
           they were invited to, into an account belonging to none. */
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(owner: currentMember, platform: platform);

        SeedDefaultSiteSubscription(context, platform);

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateMemberAdminService(context, memberEmailService: emailService.Object);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            platform: platform,
            securable: ChapterAdminSecurable.MemberImport);

        var members = new[]
        {
            new MemberImportModel { EmailAddress = "new@example.com", FirstName = "New", LastName = "Member" }
        };

        // Act
        var result = await service.ImportMembers(request, members);

        // Assert - the (mock) background task service runs the enqueued job synchronously, which reloads the
        // member, chapter and invitation and sends the email exactly once.
        result.Success.Should().BeTrue();

        emailService.Verify(
            x => x.SendMemberImportInviteEmail(
                It.IsAny<IChapterServiceRequest>(),
                It.Is<Member>(m => m.EmailAddress == "new@example.com"),
                It.IsAny<string>()),
            Times.Once);
    }

    [TestCase(PlatformType.Default)]
    [TestCase(PlatformType.DrunkenKnitwits)]
    public static async Task ImportMembers_NewMember_SavesTheImportingAdminsPlatform(PlatformType platform)
    {
        // Arrange - an imported account is raised by an admin rather than by whoever it is for, so the
        // platform it records is the one the import ran on.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(
            owner: currentMember,
            platform: platform);

        SeedDefaultSiteSubscription(context, platform);

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            platform: platform,
            securable: ChapterAdminSecurable.MemberImport);

        var members = new[]
        {
            new MemberImportModel { EmailAddress = "new@example.com", FirstName = "New", LastName = "Member" }
        };

        // Act
        var result = await service.ImportMembers(request, members);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Member>().Single(x => x.EmailAddress == "new@example.com")
            .Platform.Should().Be(platform);
    }

    [Test]
    public static async Task ImportMembers_ExistingMember_SendsTheInvitationRatherThanRecreatingTheirAccount()
    {
        /* Arrange - someone who already has an account, on either platform: there is nothing to activate, so the
           invitation is the only email that makes sense. */
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(owner: currentMember);
        var existing = context.CreateMember(afterCreate: x => x.EmailAddress = "existing@example.com");

        SeedDefaultSiteSubscription(context, PlatformType.Default);

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateMemberAdminService(context, memberEmailService: emailService.Object);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberImport);

        var members = new[]
        {
            new MemberImportModel
            {
                EmailAddress = "existing@example.com", FirstName = "Existing", LastName = "Member"
            }
        };

        // Act
        var result = await service.ImportMembers(request, members);

        // Assert
        result.Success.Should().BeTrue();

        emailService.Verify(
            x => x.SendMemberImportInviteEmail(
                It.IsAny<IChapterServiceRequest>(),
                It.Is<Member>(m => m.Id == existing.Id),
                It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public static async Task ImportMembers_MalformedEmailAddress_SkipsThatRowAndImportsTheRest()
    {
        // Arrange - a CSV is typed by hand, so a broken address is the likeliest thing in it. One bad row
        // must not create a member nobody can email, and must not stop the good rows importing either.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(owner: currentMember);

        context.Create(new SiteSubscription
        {
            Id = Guid.NewGuid(),
            Name = "Default",
            Description = "",
            GroupLimit = 10,
            Enabled = true,
            Default = true,
            Platform = PlatformType.Default,
            SitePaymentSettingId = Guid.NewGuid()
        });

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberImport);

        var members = new[]
        {
            new MemberImportModel { EmailAddress = "good@example.com", FirstName = "Good", LastName = "Member" },
            new MemberImportModel { EmailAddress = "not an email", FirstName = "Bad", LastName = "Member" }
        };

        // Act
        var result = await service.ImportMembers(request, members);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Member>().Should().Contain(x => x.EmailAddress == "good@example.com");
        context.Set<Member>().Should().NotContain(x => x.EmailAddress == "not an email");
    }

    [Test]
    public static async Task GetMemberImportPreview_MalformedEmailAddress_FlagsTheRowAsInvalid()
    {
        // Arrange - the preview is where the admin gets to see the problem, before committing.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(owner: currentMember);

        var service = CreateMemberAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberImport);

        var members = new[]
        {
            new MemberImportModel { EmailAddress = "good@example.com", FirstName = "Good", LastName = "Member" },
            new MemberImportModel { EmailAddress = "not an email", FirstName = "Bad", LastName = "Member" }
        };

        // Act
        var result = await service.GetMemberImportPreview(request, members);

        // Assert - and the good row keeps its real status rather than everything being flagged.
        result.Rows.Single(x => x.Member.EmailAddress == "not an email")
            .Status.Should().Be(MemberImportRowStatus.Invalid);
        result.Rows.Single(x => x.Member.EmailAddress == "good@example.com")
            .Status.Should().Be(MemberImportRowStatus.New);
    }

    [Test]
    public static async Task SendMemberSubscriptionReminderEmails_SendsRemindersAcrossAllChapters()
    {
        // Arrange - two published chapters, each with a member whose subscription expires within 7 days.
        // The batched load must process every chapter (the fix for the per-chapter N+1), not just one.
        using var context = CreateMockOdkContext();

        var now = DateTime.UtcNow;
        var emailService = new Mock<IMemberEmailService>();

        for (var i = 0; i < 2; i++)
        {
            var member = context.CreateMember();
            var chapter = context.CreateChapter(
                members: [member],
                afterCreate: x => x.PublishedUtc = now);

            context.Create(new ChapterMembershipSettings
            {
                ChapterId = chapter.Id,
                Enabled = true,
                MembershipDisabledAfterDaysExpired = 30
            });

            // The reminder job reads the member's current subscription from the log (source of truth).
            context.Create(new MemberSubscriptionRecord
            {
                ChapterId = chapter.Id,
                ExpiresUtc = now.AddDays(3),
                Id = Guid.NewGuid(),
                IsCurrent = true,
                MemberId = member.Id,
                PurchasedUtc = now,
                Type = SubscriptionType.Full
            });
        }

        var service = CreateMemberAdminService(context, memberEmailService: emailService.Object);
        var request = Mock.Of<IServiceRequest>(x => x.Platform == PlatformType.Default);

        // Act
        await service.SendMemberSubscriptionReminderEmails(request);

        // Assert - a reminder is sent for the expiring member in each chapter (batched across all chapters).
        emailService.Verify(
            x => x.SendMemberChapterSubscriptionExpiringEmail(
                It.IsAny<IChapterServiceRequest>(),
                It.IsAny<Member>(),
                It.IsAny<MemberChapterSubscription>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()),
            Times.Exactly(2));
    }

    private static MemberAdminService CreateMemberAdminService(
        MockOdkContext context,
        IAuthorizationService? authorizationService = null,
        IMemberEmailService? memberEmailService = null,
        IMemberImageService? memberImageService = null,
        IMemberService? memberService = null)
    {
        var unitOfWork = CreateMockUnitOfWork(context);
        var emailService = memberEmailService ?? CreateMockMemberEmailService();
        var distanceUnitFactory = new DistanceUnitFactory();
        var siteSubscriptionWriter = new MemberSiteSubscriptionWriter(unitOfWork);

        var workflow = CreateWorkflows(unitOfWork, distanceUnitFactory, siteSubscriptionWriter, emailService);

        return new MemberAdminService(
            unitOfWork,
            memberService ?? CreateMockMemberService(),
            authorizationService ?? CreateMockAuthorizationService(),
            memberImageService ?? CreateMockMemberImageService(isValid: true),
            emailService,
            new MockBackgroundTaskService(),
            new MemberChapterSubscriptionWriter(unitOfWork),
            new EmailValidationService(new InconclusiveEmailVerifier()),
            workflow.GetRequiredService<StateMachineRunner<AccountState, AccountTrigger, AccountContext>>(),
            workflow.GetRequiredService<IAccountContextFactory>(),
            workflow.GetRequiredService<StateMachineRunner<
                ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext>>(),
            workflow.GetRequiredService<IChapterMembershipContextFactory>(),
            new MockServiceRequestFactory(context),
            new SiteSubscriptionCooldown(months: 0));
    }

    /// <summary>
    /// The two machines wired the way the app wires them, over the same instances the service under test uses.
    /// Only the dependencies of the transitions these tests fire have to resolve - but the steps come from the
    /// definitions, so one added to a transition later needs no change here.
    /// </summary>
    private static IServiceProvider CreateWorkflows(
        IUnitOfWork unitOfWork,
        IDistanceUnitFactory distanceUnitFactory,
        IMemberSiteSubscriptionWriter siteSubscriptionWriter,
        IMemberEmailService memberEmailService)
    {
        var account = AccountStateMachine.Create();
        var membership = ChapterMembershipStateMachine.Create();

        var services = new ServiceCollection()
            .AddSingleton(unitOfWork)
            .AddSingleton(distanceUnitFactory)
            .AddSingleton(siteSubscriptionWriter)
            .AddSingleton(Mock.Of<IAuthorizationService>())
            .AddSingleton(memberEmailService)
            .AddSingleton(Mock.Of<IMemberImageService>())
            .AddSingleton(Mock.Of<INotificationService>())
            .AddSingleton(Mock.Of<IRecaptchaService>())
            .AddSingleton(Mock.Of<IGeolocationService>())
            .AddSingleton(Mock.Of<ITopicService>())
            .AddSingleton(Mock.Of<IOAuthProviderFactory>())
            .AddSingleton(Mock.Of<ILoggingService>())
            .AddSingleton<IEmailValidationService>(
                new EmailValidationService(new InconclusiveEmailVerifier()))
            .AddSingleton<IMemberChapterSubscriptionWriter>(new MemberChapterSubscriptionWriter(unitOfWork))
            .AddSingleton(new SiteSubscriptionCooldown(months: 0))
            .AddSingleton(account)
            .AddSingleton(membership)
            .AddScoped<IAccountContextFactory, AccountContextFactory>()
            .AddScoped<IStateResolver<AccountState, AccountContext>, AccountStateResolver>()
            .AddScoped<IStepFactory<AccountContext>, ServiceProviderStepFactory<AccountContext>>()
            .AddScoped<StateMachineRunner<AccountState, AccountTrigger, AccountContext>>()
            .AddScoped<IChapterMembershipContextFactory, ChapterMembershipContextFactory>()
            .AddScoped<
                IStateResolver<ChapterMembershipState, ChapterMembershipContext>,
                ChapterMembershipStateResolver>()
            .AddScoped<
                IStepFactory<ChapterMembershipContext>,
                ServiceProviderStepFactory<ChapterMembershipContext>>()
            .AddScoped<StateMachineRunner<
                ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext>>();

        foreach (var stepType in account.StepTypes.Concat(membership.StepTypes))
        {
            services.AddScoped(stepType);
        }

        return services.BuildServiceProvider();
    }

    private static MockOdkContext CreateMockOdkContext() => new MockOdkContext();

    private static IUnitOfWork CreateMockUnitOfWork(MockOdkContext? context = null) => MockUnitOfWorkFactory.Create(context);

    private static IAuthorizationService CreateMockAuthorizationService()
    {
        return new Mock<IAuthorizationService>().Object;
    }

    private static IMemberEmailService CreateMockMemberEmailService()
    {
        var mock = new Mock<IMemberEmailService>();
        mock.Setup(x => x.SendMemberApprovedEmail(
                It.IsAny<IChapterServiceRequest>(),
                It.IsAny<Member>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.SendMemberDeleteEmail(
                It.IsAny<IChapterServiceRequest>(),
                It.IsAny<Member>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    private static IMemberImageService CreateMockMemberImageService(bool isValid)
    {
        var mock = new Mock<IMemberImageService>();
        mock.Setup(x => x.UpdateMemberImage(It.IsAny<MemberAvatar>(), It.IsAny<byte[]>()))
            .Returns(isValid ? ServiceResult.Successful() : ServiceResult.Failure("Invalid image"));
        return mock.Object;
    }

    private static IMemberService CreateMockMemberService()
    {
        var mock = new Mock<IMemberService>();
        mock.Setup(x => x.DeleteMemberChapterData(It.IsAny<IMemberChapterServiceRequest>()))
            .ReturnsAsync(ServiceResult.Successful());
        mock.Setup(x => x.RotateMemberImage(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    private static MemberChapterInvite CreateInvite(
        MockOdkContext context, Guid chapterId, Guid memberId, DateTime createdUtc) => context.Create(
        new MemberChapterInvite
        {
            ChapterId = chapterId,
            CreatedUtc = createdUtc,
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Token = Guid.NewGuid().ToString()
        });

    private static IMemberChapterAdminServiceRequest CreateMemberChapterAdminServiceRequest(
        Chapter chapter,
        Member currentMember,
        PlatformType? platform = null,
        ChapterAdminSecurable? securable = null)
    {
        var mock = new Mock<IMemberChapterAdminServiceRequest>();

        mock.Setup(x => x.Chapter)
            .Returns(chapter);

        mock.Setup(x => x.CurrentMember)
            .Returns(currentMember);

        mock.Setup(x => x.CurrentMemberOrDefault)
            .Returns(currentMember);

        mock.Setup(x => x.HttpRequestContext)
            .Returns(CreateHttpRequestContext());

        mock.Setup(x => x.Platform)
            .Returns(platform ?? PlatformType.Default);

        mock.Setup(x => x.Securable)
            .Returns(securable ?? ChapterAdminSecurable.Any);

        return mock.Object;
    }

    private static IHttpRequestContext CreateHttpRequestContext(string? baseUrl = null)
    {
        var mock = new Mock<IHttpRequestContext>();
        mock.Setup(m => m.BaseUrl)
            .Returns(baseUrl ?? "https://test.local");
        return mock.Object;
    }

    private static ChapterMembershipSettings CreateChapterMembershipSettings(
        Chapter chapter,
        bool? enabled = null)
    {
        return new ChapterMembershipSettings
        {
            ChapterId = chapter.Id,
            Enabled = enabled ?? true,
            MembershipDisabledAfterDaysExpired = 7
        };
    }

    private static Notification CreateNotification(
        Member member,
        NotificationType? type = null,
        Guid? entityId = null)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            Type = type ?? NotificationType.NewMember,
            ReadUtc = null,
            CreatedUtc = DateTime.UtcNow,
            EntityId = entityId
        };
    }

    // The import reads the platform's default subscription whether or not it creates a member.
    private static void SeedDefaultSiteSubscription(MockOdkContext context, PlatformType platform)
    {
        context.Create(new SiteSubscription
        {
            Default = true,
            Description = "",
            Enabled = true,
            GroupLimit = 10,
            Id = Guid.NewGuid(),
            Name = "Default",
            Platform = platform,
            SitePaymentSettingId = Guid.NewGuid()
        });
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Security;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Members;

[Parallelizable]
public static class MemberAdminServiceTests
{
    [Test]
    public static async Task ApproveMember_WhenMemberExists_ApprovesMember()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, member) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: currentMember,
            unapprovedMembers: [member]);

        var service = CreateMemberAdminService(context);

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

        context.Create(CreateMemberSubscription(
            member.MemberChapter(chapter.Id),
            type: SubscriptionType.Full,
            expiresUtc: DateTime.UtcNow.AddDays(10)));

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

        var subscription = context.Set<MemberSubscription>()
            .Single(x => x.MemberChapter.MemberId == member.Id && x.MemberChapter.ChapterId == chapter.Id);
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

        context.Create(CreateMemberSubscription(member.MemberChapter(chapter.Id)));

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

    private static MemberAdminService CreateMemberAdminService(
        MockOdkContext context,
        IAuthorizationService? authorizationService = null,
        IMemberEmailService? memberEmailService = null,
        IMemberImageService? memberImageService = null,
        IMemberService? memberService = null)
    {
        return new MemberAdminService(
            CreateMockUnitOfWork(context),
            memberService ?? CreateMockMemberService(),
            authorizationService ?? CreateMockAuthorizationService(),
            memberImageService ?? CreateMockMemberImageService(isValid: true),
            memberEmailService ?? CreateMockMemberEmailService());
    }

    private static MockOdkContext CreateMockOdkContext() => new MockOdkContext();

    private static IUnitOfWork CreateMockUnitOfWork(MockOdkContext? context = null) => MockUnitOfWork.Create(context);

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

    private static MemberSubscription CreateMemberSubscription(
        MemberChapter? memberChapter,
        SubscriptionType? type = null,
        DateTime? expiresUtc = null)
    {
        return new MemberSubscription
        {
            MemberChapter = memberChapter!,
            MemberChapterId = memberChapter!.Id,
            Type = type ?? SubscriptionType.Full,
            ExpiresUtc = expiresUtc
        };
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
}
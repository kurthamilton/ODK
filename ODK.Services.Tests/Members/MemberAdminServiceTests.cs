using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.Core.Repositories;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Security;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Members;

[Parallelizable]
public static class MemberAdminServiceTests
{
    private static readonly Guid CurrentMemberId = Guid.NewGuid();

    [Test]
    public static async Task ApproveMember_WhenMemberExists_ApprovesMember()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);
        var memberId = Guid.NewGuid();
        var member = CreateMember(id: memberId, chapter: chapter);

        var adminMember = CreateChapterAdminMember(member: currentMember, chapterId: chapter.Id, role: ChapterAdminRole.Owner);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var subscription = CreateMemberSiteSubscription();
        var memberSubscriptionRepository = CreateMockMemberSiteSubscriptionRepository(subscription);

        var memberRepository = CreateMockMemberRepository([member]);
        var memberChapterRepository = CreateMockMemberChapterRepository();

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberChapterRepository: memberChapterRepository);

        var service = CreateMemberAdminService(unitOfWork);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberApprovals);

        // Act
        var result = await service.ApproveMember(request, memberId);

        // Assert
        result.Success.Should().BeTrue();
        Mock.Get(memberChapterRepository).Verify(x => x.Update(It.IsAny<MemberChapter>()), Times.Once);
    }

    [Test]
    public static async Task ApproveMember_SetsMemberChapterApprovedToTrue()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);
        var member = CreateMember(chapter: chapter);

        var adminMember = CreateChapterAdminMember(member: currentMember, chapterId: chapter.Id, role: ChapterAdminRole.Owner);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var memberRepository = CreateMockMemberRepository([member]);

        MemberChapter? updatedMemberChapter = null;
        var memberChapterRepository = CreateMockMemberChapterRepository(
            onUpdate: mc => updatedMemberChapter = mc);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberChapterRepository: memberChapterRepository);

        var service = CreateMemberAdminService(unitOfWork);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberApprovals);

        // Act
        await service.ApproveMember(request, member.Id);

        // Assert
        updatedMemberChapter.Should().NotBeNull();
        updatedMemberChapter!.Approved.Should().BeTrue();
    }

    [Test]
    public static async Task GetAdminMemberViewModel_WhenValid_ReturnsViewModel()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);
        var memberId = Guid.NewGuid();
        var targetMember = CreateMember(id: memberId);

        var currentAdminMember = CreateChapterAdminMember(
            member: currentMember,
            chapterId: chapter.Id,
            role: ChapterAdminRole.Owner);
        var targetAdminMember = CreateChapterAdminMember(
            member: targetMember,
            chapterId: chapter.Id,
            role: ChapterAdminRole.Admin);

        var adminMemberRepository = CreateMockChapterAdminMemberRepository(
            [currentAdminMember, targetAdminMember]);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: adminMemberRepository);

        var service = CreateMemberAdminService(unitOfWork);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetAdminMemberViewModel(request, memberId);

        // Assert
        result.Should().NotBeNull();
        result.AdminMember.MemberId.Should().Be(memberId);
        result.ReadOnly.Should().BeFalse();
        result.CanEditRole.Should().BeTrue();
    }

    [Test]
    public static async Task GetAdminMemberViewModel_WhenOwner_IsReadOnly_ForNonOwner()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);
        var memberId = Guid.NewGuid();
        var targetMember = CreateMember(id: memberId);

        var currentAdminMember = CreateChapterAdminMember(
            member: currentMember,
            chapterId: chapter.Id,
            role: ChapterAdminRole.Admin);
        var targetAdminMember = CreateChapterAdminMember(
            member: targetMember,
            chapterId: chapter.Id,
            role: ChapterAdminRole.Owner);

        var adminMemberRepository = CreateMockChapterAdminMemberRepository(
            [currentAdminMember, targetAdminMember]);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: adminMemberRepository);

        var service = CreateMemberAdminService(unitOfWork);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetAdminMemberViewModel(request, memberId);

        // Assert
        result.ReadOnly.Should().BeTrue();
        result.CanEditRole.Should().BeFalse();
    }

    [Test]
    public static async Task GetAdminMemberViewModel_WhenOwner_CannotEditRole()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);

        var adminMember = CreateChapterAdminMember(
            member: currentMember,
            chapterId: chapter.Id,
            role: ChapterAdminRole.Owner);
        var adminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: adminMemberRepository);

        var service = CreateMemberAdminService(unitOfWork);

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
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);
        var memberId = Guid.NewGuid();
        var targetMember = CreateMember(id: memberId);

        var currentAdminMember = CreateChapterAdminMember(
            member: currentMember,
            chapterId: chapter.Id,
            role: ChapterAdminRole.Admin);
        var targetAdminMember = CreateChapterAdminMember(
            member: targetMember,
            chapterId: chapter.Id,
            role: ChapterAdminRole.Admin);

        var adminMemberRepository = CreateMockChapterAdminMemberRepository([currentAdminMember, targetAdminMember]);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: adminMemberRepository);

        var service = CreateMemberAdminService(unitOfWork);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetAdminMemberViewModel(request, memberId);

        // Assert
        result.ReadOnly.Should().BeFalse();
        result.CanEditRole.Should().BeFalse();
    }

    [Test]
    public static async Task GetAdminMemberViewModel_CanEditOwnRole()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);

        var adminMember = CreateChapterAdminMember(
            member: currentMember,
            chapterId: chapter.Id,
            role: ChapterAdminRole.Admin);
        var adminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: adminMemberRepository);

        var service = CreateMemberAdminService(unitOfWork);

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
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);

        var adminMember = CreateChapterAdminMember(member: currentMember, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var members = new[] { CreateMember(chapter: chapter), CreateMember(chapter: chapter) };
        var memberRepository = CreateMockMemberRepository(members: members);

        var membershipSettings = CreateChapterMembershipSettings(chapterId: chapter.Id);
        var membershipSettingsRepository = CreateMockChapterMembershipSettingsRepository(membershipSettings);

        var subscriptions = new[] { CreateMemberSubscription() };
        var memberSubscriptionRepository = CreateMockMemberSubscriptionRepository(subscriptions: subscriptions);

        var emailPreferences = Array.Empty<MemberEmailPreference>();
        var emailPreferenceRepository = CreateMockMemberEmailPreferenceRepository(emailPreferences);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            membershipSettingsRepository: membershipSettingsRepository,
            memberSubscriptionRepository: memberSubscriptionRepository,
            memberEmailPreferenceRepository: emailPreferenceRepository);

        var service = CreateMemberAdminService(unitOfWork);

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
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);
        var memberId = Guid.NewGuid();
        var member = CreateMember(id: memberId, chapter: chapter);

        var adminMember = CreateChapterAdminMember(member: currentMember, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var memberRepository = CreateMockMemberRepository([member]);
        var subscriptionRepository = CreateMockMemberSubscriptionRepository();
        var notificationRepository = CreateMockNotificationRepository();

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberSubscriptionRepository: subscriptionRepository,
            notificationRepository: notificationRepository);

        var service = CreateMemberAdminService(unitOfWork);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Members);

        // Act
        var result = await service.GetMemberViewModel(request, memberId);

        // Assert
        result.Should().NotBeNull();
        result.Member.Id.Should().Be(memberId);
        result.Chapter.Should().Be(chapter);
    }

    [Test]
    public static async Task GetMemberViewModel_MarksNotificationsAsRead()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);
        var memberId = Guid.NewGuid();
        var member = CreateMember(id: memberId, chapter: chapter);

        var adminMember = CreateChapterAdminMember(member: currentMember, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var notification = CreateNotification();
        var memberRepository = CreateMockMemberRepository([member]);
        var subscriptionRepository = CreateMockMemberSubscriptionRepository();
        var notificationRepository = CreateMockNotificationRepository([notification]);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberSubscriptionRepository: subscriptionRepository,
            notificationRepository: notificationRepository);

        var service = CreateMemberAdminService(unitOfWork);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Members);

        // Act
        await service.GetMemberViewModel(request, memberId);

        // Assert
        Mock.Get(notificationRepository).Verify(x => x.MarkAsRead(It.IsAny<IReadOnlyCollection<Notification>>()), Times.Once);
    }

    [Test]
    public static async Task GetMemberApprovalsViewModel_ReturnsPendingMembers()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);

        var adminMember = CreateChapterAdminMember(member: currentMember, chapterId: chapter.Id, role: ChapterAdminRole.Owner);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var approvedMember = CreateMember(id: Guid.NewGuid(), chapter: chapter, approved: true);
        var pendingMember = CreateMember(id: Guid.NewGuid(), chapter: chapter, approved: false);
        var members = new[] { approvedMember, pendingMember };

        var memberRepository = CreateMockMemberRepository(members: members);

        var membershipSettings = CreateChapterMembershipSettings(chapterId: chapter.Id);
        var membershipSettingsRepository = CreateMockChapterMembershipSettingsRepository(membershipSettings);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            membershipSettingsRepository: membershipSettingsRepository);

        var service = CreateMemberAdminService(unitOfWork);

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
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);
        var memberId = Guid.NewGuid();
        var member = CreateMember(id: memberId, chapter: chapter);

        var adminMember = CreateChapterAdminMember(member: currentMember, chapterId: chapter.Id, role: ChapterAdminRole.Owner);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var subscription = CreateMemberSubscription(
            type: SubscriptionType.Full,
            expiresUtc: DateTime.UtcNow.AddDays(10));

        var memberRepository = CreateMockMemberRepository([member]);
        var subscriptionRepository = CreateMockMemberSubscriptionRepository(subscriptions: [subscription]);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberSubscriptionRepository: subscriptionRepository);

        var service = CreateMemberAdminService(unitOfWork);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberApprovals);

        // Act
        var result = await service.RemoveMemberFromChapter(request, memberId, "test reason");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("cannot remove");
    }

    [Test]
    public static async Task RemoveMemberFromChapter_WhenNoSubscription_ReturnsSuccess()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);
        var memberId = Guid.NewGuid();
        var member = CreateMember(id: memberId, chapter: chapter);

        var adminMember = CreateChapterAdminMember(member: currentMember, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var memberRepository = CreateMockMemberRepository([member]);
        var subscriptionRepository = CreateMockMemberSubscriptionRepository();
        var memberService = CreateMockMemberService();
        var memberEmailService = CreateMockMemberEmailService();

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberSubscriptionRepository: subscriptionRepository);

        var service = CreateMemberAdminService(
            unitOfWork,
            memberService: memberService,
            memberEmailService: memberEmailService);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberApprovals);

        // Act
        var result = await service.RemoveMemberFromChapter(request, memberId, "test reason");

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateMemberSubscription_WhenValid_UpdatesSuccessfully()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);
        var memberId = Guid.NewGuid();
        var member = CreateMember(id: memberId, chapter: chapter);

        var adminMember = CreateChapterAdminMember(member: currentMember, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var memberRepository = CreateMockMemberRepository([member]);
        var subscriptionRepository = CreateMockMemberSubscriptionRepository();

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberSubscriptionRepository: subscriptionRepository);

        var service = CreateMemberAdminService(unitOfWork);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberAdmin);

        var model = new MemberSubscriptionUpdateModel
        {
            Type = SubscriptionType.Full,
            ExpiryDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = await service.UpdateMemberSubscription(request, memberId, model);

        // Assert
        result.Success.Should().BeTrue();
        Mock.Get(subscriptionRepository).Verify(
            x => x.Add(It.IsAny<MemberSubscription>()), Times.Once);
    }

    [Test]
    public static async Task UpdateMemberSubscription_WhenInvalidType_ReturnsFails()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);
        var memberId = Guid.NewGuid();
        var member = CreateMember(id: memberId, chapter: chapter);

        var adminMember = CreateChapterAdminMember(member: currentMember, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var memberRepository = CreateMockMemberRepository([member]);
        var subscriptionRepository = CreateMockMemberSubscriptionRepository();

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberSubscriptionRepository: subscriptionRepository);

        var service = CreateMemberAdminService(unitOfWork);

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
        var result = await service.UpdateMemberSubscription(request, memberId, model);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid type");
    }

    [Test]
    public static async Task UpdateMemberImage_WhenValid_UpdatesSuccessfully()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);
        var memberId = Guid.NewGuid();
        var member = CreateMember(id: memberId, chapter: chapter);

        var adminMember = CreateChapterAdminMember(member: currentMember, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var memberRepository = CreateMockMemberRepository([member]);
        var imageRepository = CreateMockMemberImageRepository();
        var avatarRepository = CreateMockMemberAvatarRepository();
        var cacheService = CreateMockCacheService();
        var memberImageService = CreateMockMemberImageService(isValid: true);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberImageRepository: imageRepository,
            memberAvatarRepository: avatarRepository);

        var service = CreateMemberAdminService(
            unitOfWork,
            cacheService: cacheService,
            memberImageService: memberImageService);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberImage);

        var model = new MemberImageUpdateModel { ImageData = [1, 2, 3] };

        // Act
        var result = await service.UpdateMemberImage(request, memberId, model);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Picture updated");
    }

    [Test]
    public static async Task UpdateMemberImage_WhenInvalidImage_ReturnsFails()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);
        var memberId = Guid.NewGuid();
        var member = CreateMember(id: memberId, chapter: chapter);

        var adminMember = CreateChapterAdminMember(member: currentMember, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var memberRepository = CreateMockMemberRepository([member]);
        var imageRepository = CreateMockMemberImageRepository();
        var avatarRepository = CreateMockMemberAvatarRepository();
        var memberImageService = CreateMockMemberImageService(isValid: false);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberImageRepository: imageRepository,
            memberAvatarRepository: avatarRepository);

        var service = CreateMemberAdminService(
            unitOfWork,
            memberImageService: memberImageService);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MemberImage);

        var model = new MemberImageUpdateModel { ImageData = [1, 2, 3] };

        // Act
        var result = await service.UpdateMemberImage(request, memberId, model);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public static async Task GetMemberCsv_ReturnsMembersInCsvFormat()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId);
        var member = CreateMember(id: Guid.NewGuid(), chapter: chapter);

        var adminMember = CreateChapterAdminMember(member: currentMember, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var members = new[] { member };
        var memberRepository = CreateMockMemberRepository(members: members);

        var subscriptions = new[] { CreateMemberSubscription() };
        var subscriptionRepository = CreateMockMemberSubscriptionRepository(subscriptions: subscriptions);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberSubscriptionRepository: subscriptionRepository);

        var service = CreateMemberAdminService(unitOfWork);

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
        IUnitOfWork unitOfWork,
        IAuthorizationService? authorizationService = null,
        ICacheService? cacheService = null,
        IMemberEmailService? memberEmailService = null,
        IMemberImageService? memberImageService = null,
        IMemberService? memberService = null)
    {
        return new MemberAdminService(
            unitOfWork,
            memberService ?? CreateMockMemberService(),
            cacheService ?? CreateMockCacheService(),
            authorizationService ?? CreateMockAuthorizationService(),
            memberImageService ?? CreateMockMemberImageService(isValid: true),
            memberEmailService ?? CreateMockMemberEmailService());
    }

    private static MemberSiteSubscription CreateMemberSiteSubscription(
        DateTime? expiresUtc = null,
        int? groupLimit = null,
        SiteFeatureType? feature = null)
    {
        var siteSubscription = new SiteSubscription
        {
            Id = Guid.NewGuid(),
            Name = "Test Subscription",
            Description = "Test subscription for testing",
            GroupLimit = groupLimit ?? 10,
            Enabled = true,
            Default = false,
            Platform = PlatformType.Default,
            SitePaymentSettingId = Guid.NewGuid(),
            Features = feature.HasValue
                ? [new SiteSubscriptionFeature { Feature = feature.Value }]
                : []
        };

        return new MemberSiteSubscription
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            SiteSubscriptionId = siteSubscription.Id,
            ExpiresUtc = expiresUtc,
            SiteSubscription = siteSubscription
        };
    }

    private static MockUnitOfWork CreateMockUnitOfWork(
        IMemberRepository? memberRepository = null,
        IMemberChapterRepository? memberChapterRepository = null,
        IMemberSubscriptionRepository? memberSubscriptionRepository = null,
        IMemberImageRepository? memberImageRepository = null,
        IMemberAvatarRepository? memberAvatarRepository = null,
        IChapterAdminMemberRepository? chapterAdminMemberRepository = null,
        IChapterMembershipSettingsRepository? membershipSettingsRepository = null,
        IMemberEmailPreferenceRepository? memberEmailPreferenceRepository = null,
        INotificationRepository? notificationRepository = null,
        IEventRepository? eventRepository = null,
        IVenueRepository? venueRepository = null,
        IEventResponseRepository? eventResponseRepository = null,
        IEventInviteRepository? eventInviteRepository = null,
        IChapterConversationRepository? conversationRepository = null,
        IMemberSiteSubscriptionRepository? memberSiteSubscriptionRepository = null,
        IPaymentRepository? paymentRepository = null,
        IChapterPaymentSettingsRepository? chapterPaymentSettingsRepository = null,
        IChapterPaymentAccountRepository? chapterPaymentAccountRepository = null,
        ICurrencyRepository? currencyRepository = null,
        ISitePaymentSettingsRepository? sitePaymentSettingsRepository = null,
        IChapterSubscriptionRepository? chapterSubscriptionRepository = null)
    {
        var mock = new Mock<IUnitOfWork>();

        mock.Setup(x => x.MemberRepository)
            .Returns(memberRepository ?? CreateMockMemberRepository());
        mock.Setup(x => x.MemberChapterRepository)
            .Returns(memberChapterRepository ?? CreateMockMemberChapterRepository());
        mock.Setup(x => x.MemberSubscriptionRepository)
            .Returns(memberSubscriptionRepository ?? CreateMockMemberSubscriptionRepository());
        mock.Setup(x => x.MemberImageRepository)
            .Returns(memberImageRepository ?? CreateMockMemberImageRepository());
        mock.Setup(x => x.MemberAvatarRepository)
            .Returns(memberAvatarRepository ?? CreateMockMemberAvatarRepository());
        mock.Setup(x => x.ChapterAdminMemberRepository)
            .Returns(chapterAdminMemberRepository ?? CreateMockChapterAdminMemberRepository());
        mock.Setup(x => x.ChapterMembershipSettingsRepository)
            .Returns(membershipSettingsRepository ?? CreateMockChapterMembershipSettingsRepository());
        mock.Setup(x => x.MemberEmailPreferenceRepository)
            .Returns(memberEmailPreferenceRepository ?? CreateMockMemberEmailPreferenceRepository());
        mock.Setup(x => x.NotificationRepository)
            .Returns(notificationRepository ?? CreateMockNotificationRepository());
        mock.Setup(x => x.EventRepository)
            .Returns(eventRepository ?? CreateMockEventRepository());
        mock.Setup(x => x.VenueRepository)
            .Returns(venueRepository ?? CreateMockVenueRepository());
        mock.Setup(x => x.EventResponseRepository)
            .Returns(eventResponseRepository ?? CreateMockEventResponseRepository());
        mock.Setup(x => x.EventInviteRepository)
            .Returns(eventInviteRepository ?? CreateMockEventInviteRepository());
        mock.Setup(x => x.ChapterConversationRepository)
            .Returns(conversationRepository ?? CreateMockChapterConversationRepository());
        mock.Setup(x => x.MemberSiteSubscriptionRepository)
            .Returns(memberSiteSubscriptionRepository ?? CreateMockMemberSiteSubscriptionRepository());
        mock.Setup(x => x.PaymentRepository)
            .Returns(paymentRepository ?? CreateMockPaymentRepository());
        mock.Setup(x => x.ChapterPaymentSettingsRepository)
            .Returns(chapterPaymentSettingsRepository ?? CreateMockChapterPaymentSettingsRepository());
        mock.Setup(x => x.ChapterPaymentAccountRepository)
            .Returns(chapterPaymentAccountRepository ?? CreateMockChapterPaymentAccountRepository());
        mock.Setup(x => x.CurrencyRepository)
            .Returns(currencyRepository ?? CreateMockCurrencyRepository());
        mock.Setup(x => x.SitePaymentSettingsRepository)
            .Returns(sitePaymentSettingsRepository ?? CreateMockSitePaymentSettingsRepository());
        mock.Setup(x => x.ChapterSubscriptionRepository)
            .Returns(chapterSubscriptionRepository ?? CreateMockChapterSubscriptionRepository());

        return new MockUnitOfWork(mock);
    }

    private static IMemberRepository CreateMockMemberRepository(IEnumerable<Member>? members = null)
    {
        var mock = new Mock<IMemberRepository>();
        members ??= [];

        mock.Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns((Guid memberId) =>
                new MockDeferredQuerySingle<Member>(
                    members.FirstOrDefault(x => x.Id == memberId)));

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns((Guid chapterId) =>
                new MockDeferredQueryMultiple<Member>(
                    members.Where(x => x.IsMemberOf(chapterId))));

        mock.Setup(x => x.GetAllByChapterId(It.IsAny<Guid>()))
            .Returns((Guid chapterId) =>
                new MockDeferredQueryMultiple<Member>(
                    members.Where(x => x.IsMemberOf(chapterId))));

        return mock.Object;
    }

    private static IMemberChapterRepository CreateMockMemberChapterRepository(Action<MemberChapter>? onUpdate = null)
    {
        var mock = new Mock<IMemberChapterRepository>();

        mock.Setup(x => x.Update(It.IsAny<MemberChapter>()))
            .Callback((MemberChapter mc) => onUpdate?.Invoke(mc));

        return mock.Object;
    }

    private static IMemberSubscriptionRepository CreateMockMemberSubscriptionRepository(
        IEnumerable<MemberSubscription>? subscriptions = null)
    {
        var mock = new Mock<IMemberSubscriptionRepository>();
        subscriptions ??= [];

        mock.Setup(x => x.GetByMemberId(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns((Guid memberId, Guid chapterId) =>
                new MockDeferredQuerySingleOrDefault<MemberSubscription>(
                    subscriptions.FirstOrDefault()));

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns((Guid chapterId) =>
                new MockDeferredQueryMultiple<MemberSubscription>(subscriptions));

        mock.Setup(x => x.Add(It.IsAny<MemberSubscription>()));
        mock.Setup(x => x.Update(It.IsAny<MemberSubscription>()));

        return mock.Object;
    }

    private static IMemberImageRepository CreateMockMemberImageRepository()
    {
        var mock = new Mock<IMemberImageRepository>();

        mock.Setup(x => x.GetByMemberId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingleOrDefault<MemberImage>(null));

        mock.Setup(x => x.Add(It.IsAny<MemberImage>()));
        mock.Setup(x => x.Update(It.IsAny<MemberImage>()));

        return mock.Object;
    }

    private static IMemberAvatarRepository CreateMockMemberAvatarRepository()
    {
        var mock = new Mock<IMemberAvatarRepository>();

        mock.Setup(x => x.GetByMemberId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingleOrDefault<MemberAvatar>(null));

        mock.Setup(x => x.Add(It.IsAny<MemberAvatar>()));
        mock.Setup(x => x.Update(It.IsAny<MemberAvatar>()));

        return mock.Object;
    }

    private static IChapterAdminMemberRepository CreateMockChapterAdminMemberRepository(
        IEnumerable<ChapterAdminMember>? adminMembers = null)
    {
        var mock = new Mock<IChapterAdminMemberRepository>();
        adminMembers ??= [];

        mock.Setup(x => x.GetByChapterId(It.IsAny<PlatformType>(), It.IsAny<Guid>()))
            .Returns((PlatformType platform, Guid chapterId) =>
                new MockDeferredQueryMultiple<ChapterAdminMember>(
                    adminMembers.Where(x => x.ChapterId == chapterId)));

        mock.Setup(x => x.GetByMemberId(It.IsAny<PlatformType>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns((PlatformType platform, Guid memberId, Guid chapterId) =>
                new MockDeferredQuerySingleOrDefault<ChapterAdminMember>(
                    adminMembers.FirstOrDefault(x => x.ChapterId == chapterId && x.MemberId == memberId)));

        return mock.Object;
    }

    private static IChapterMembershipSettingsRepository CreateMockChapterMembershipSettingsRepository(
        ChapterMembershipSettings? settings = null)
    {
        var mock = new Mock<IChapterMembershipSettingsRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingleOrDefault<ChapterMembershipSettings>(settings));

        return mock.Object;
    }

    private static IMemberEmailPreferenceRepository CreateMockMemberEmailPreferenceRepository(
        IEnumerable<MemberEmailPreference>? preferences = null)
    {
        var mock = new Mock<IMemberEmailPreferenceRepository>();
        preferences ??= [];

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>(), It.IsAny<MemberEmailPreferenceType>()))
            .Returns((Guid chapterId, MemberEmailPreferenceType type) =>
                new MockDeferredQueryMultiple<MemberEmailPreference>(preferences));

        return mock.Object;
    }

    private static INotificationRepository CreateMockNotificationRepository(
        IEnumerable<Notification>? notifications = null)
    {
        var mock = new Mock<INotificationRepository>();
        notifications ??= [];

        mock.Setup(x => x.GetUnreadByMemberId(It.IsAny<Guid>(), It.IsAny<NotificationType>(), It.IsAny<Guid>()))
            .Returns(new MockDeferredQueryMultiple<Notification>(notifications));

        mock.Setup(x => x.MarkAsRead(It.IsAny<IReadOnlyCollection<Notification>>()));

        return mock.Object;
    }

    private static IEventRepository CreateMockEventRepository()
    {
        return new Mock<IEventRepository>().Object;
    }

    private static IVenueRepository CreateMockVenueRepository()
    {
        return new Mock<IVenueRepository>().Object;
    }

    private static IEventResponseRepository CreateMockEventResponseRepository()
    {
        return new Mock<IEventResponseRepository>().Object;
    }

    private static IEventInviteRepository CreateMockEventInviteRepository()
    {
        return new Mock<IEventInviteRepository>().Object;
    }

    private static IChapterConversationRepository CreateMockChapterConversationRepository()
    {
        return new Mock<IChapterConversationRepository>().Object;
    }

    private static IMemberSiteSubscriptionRepository CreateMockMemberSiteSubscriptionRepository(
        MemberSiteSubscription? chapterSubscription = null,
        MemberSiteSubscription? memberSubscription = null)
    {
        var mock = new Mock<IMemberSiteSubscriptionRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns((Guid chapterId) =>
                new MockDeferredQuerySingleOrDefault<MemberSiteSubscription>(chapterSubscription));

        mock.Setup(x => x.GetByMemberId(It.IsAny<Guid>(), It.IsAny<PlatformType>()))
            .Returns((Guid memberId, PlatformType platform) =>
                new MockDeferredQuerySingleOrDefault<MemberSiteSubscription>(memberSubscription));

        return mock.Object;
    }

    private static IPaymentRepository CreateMockPaymentRepository()
    {
        return new Mock<IPaymentRepository>().Object;
    }

    private static IChapterPaymentSettingsRepository CreateMockChapterPaymentSettingsRepository()
    {
        return new Mock<IChapterPaymentSettingsRepository>().Object;
    }

    private static IChapterPaymentAccountRepository CreateMockChapterPaymentAccountRepository()
    {
        return new Mock<IChapterPaymentAccountRepository>().Object;
    }

    private static ICurrencyRepository CreateMockCurrencyRepository()
    {
        return new Mock<ICurrencyRepository>().Object;
    }

    private static ISitePaymentSettingsRepository CreateMockSitePaymentSettingsRepository()
    {
        return new Mock<ISitePaymentSettingsRepository>().Object;
    }

    private static IChapterSubscriptionRepository CreateMockChapterSubscriptionRepository()
    {
        return new Mock<IChapterSubscriptionRepository>().Object;
    }

    private static IAuthorizationService CreateMockAuthorizationService()
    {
        return new Mock<IAuthorizationService>().Object;
    }

    private static ICacheService CreateMockCacheService()
    {
        return new Mock<ICacheService>().Object;
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
        mock.Setup(x => x.UpdateMemberImage(It.IsAny<MemberImage>(), It.IsAny<MemberAvatar>(), It.IsAny<byte[]>()))
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
        Chapter? chapter = null,
        Member? currentMember = null,
        PlatformType? platform = null,
        ChapterAdminSecurable? securable = null)
    {
        currentMember ??= CreateMember(id: CurrentMemberId);

        var mock = new Mock<IMemberChapterAdminServiceRequest>();

        mock.Setup(x => x.Chapter)
            .Returns(chapter ?? CreateChapter());

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

    private static Chapter CreateChapter(Guid? id = null, string? name = null, Guid? ownerId = null)
    {
        var slugName = name ?? "Test Chapter";
        return new Chapter
        {
            Id = id ?? Guid.NewGuid(),
            Name = slugName,
            Slug = slugName.ToLowerInvariant().Replace(" ", "-"),
            OwnerId = ownerId,
            CreatedUtc = DateTime.UtcNow,
            CountryId = Guid.NewGuid(),
            Platform = PlatformType.Default
        };
    }

    private static Member CreateMember(
        Guid? id = null,
        Chapter? chapter = null,
        bool? approved = null)
    {
        var member = new Member
        {
            Id = id ?? Guid.NewGuid(),
            SiteAdmin = false,
            Activated = true
        };

        if (chapter != null)
        {
            var memberChapter = new MemberChapter
            {
                Id = Guid.NewGuid(),
                ChapterId = chapter.Id,
                MemberId = member.Id,
                Approved = approved ?? true,
                CreatedUtc = DateTime.UtcNow
            };
            member.Chapters.Add(memberChapter);
        }

        return member;
    }

    private static ChapterAdminMember CreateChapterAdminMember(
        Member? member = null,
        Guid? chapterId = null,
        ChapterAdminRole? role = null)
    {
        member ??= CreateMember();

        return new ChapterAdminMember
        {
            Id = Guid.NewGuid(),
            Member = member,
            MemberId = member.Id,
            ChapterId = chapterId ?? Guid.NewGuid(),
            Role = role ?? ChapterAdminRole.Admin
        };
    }

    private static MemberSubscription CreateMemberSubscription(
        Guid? memberId = null,
        SubscriptionType? type = null,
        DateTime? expiresUtc = null)
    {
        var memberChapter = new MemberChapter
        {
            Id = Guid.NewGuid(),
            ChapterId = Guid.NewGuid(),
            MemberId = memberId ?? Guid.NewGuid(),
            CreatedUtc = DateTime.UtcNow
        };

        return new MemberSubscription
        {
            MemberChapter = memberChapter,
            MemberChapterId = memberChapter.Id,
            Type = type ?? SubscriptionType.Full,
            ExpiresUtc = expiresUtc
        };
    }

    private static ChapterMembershipSettings CreateChapterMembershipSettings(
        Guid? chapterId = null,
        bool? enabled = null)
    {
        return new ChapterMembershipSettings
        {
            ChapterId = chapterId ?? Guid.NewGuid(),
            Enabled = enabled ?? true,
            MembershipDisabledAfterDaysExpired = 7
        };
    }

    private static Notification CreateNotification(
        Guid? id = null,
        Guid? memberId = null,
        NotificationType? type = null)
    {
        return new Notification
        {
            Id = id ?? Guid.NewGuid(),
            MemberId = memberId ?? Guid.NewGuid(),
            Type = type ?? NotificationType.NewMember,
            ReadUtc = null,
            CreatedUtc = DateTime.UtcNow
        };
    }
}
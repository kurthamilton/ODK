using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.DataTypes;
using ODK.Core.Emails;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.Core.Repositories;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Exceptions;
using ODK.Services.Geolocation;
using ODK.Services.Imaging;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Notifications;
using ODK.Services.Payments;
using ODK.Services.Security;
using ODK.Services.SocialMedia;
using ODK.Services.Subscriptions;
using ODK.Services.Tests.Helpers;
using ODK.Services.Topics;
using ODK.Services.Topics.Models;
using ODK.Services.Web;

namespace ODK.Services.Tests.Chapters;

[TestFixture]
public static class ChapterAdminServiceTests
{
    [Test]
    public static void AddChapterAdminMember_WhenMemberNotChapterAdmin_ThrowsException()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var currentMemberId = Guid.NewGuid();
        var chapter = CreateChapter();
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository();
        var memberRepository = CreateMockMemberRepository(new[] { CreateMember(id: currentMemberId), CreateMember(id: memberId) });
        var subscription = CreateMemberSiteSubscription(feature: SiteFeatureType.AdminMembers);
        var memberSubscriptionRepository = CreateMockMemberSiteSubscriptionRepository(subscription);
        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberSiteSubscriptionRepository: memberSubscriptionRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act & Assert
        Assert.ThrowsAsync<OdkNotAuthorizedException>(async () => await service.AddChapterAdminMember(request, memberId));
    }

    [Test]
    public static async Task AddChapterAdminMember_WhenFeatureNotEnabled_ReturnsFailure()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var currentMemberId = Guid.NewGuid();
        var chapter = CreateChapter();
        var adminMembers = new List<ChapterAdminMember> { CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id) };
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(adminMembers);
        var memberRepository = CreateMockMemberRepository(new[] { CreateMember(id: currentMemberId), CreateMember(id: memberId) });
        var subscription = CreateMemberSiteSubscription();
        var memberSubscriptionRepository = CreateMockMemberSiteSubscriptionRepository(subscription);
        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberSiteSubscriptionRepository: memberSubscriptionRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act
        var result = await service.AddChapterAdminMember(request, memberId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Not permitted");
    }

    [Test]
    public static async Task AddChapterAdminMember_WhenMemberAlreadyAdmin_ReturnsFailure()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var currentMemberId = Guid.NewGuid();
        var chapter = CreateChapter();
        var adminMembers = new List<ChapterAdminMember>
        {
            CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id),
            CreateChapterAdminMember(currentMemberId: memberId, chapterId: chapter.Id)
        };
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(adminMembers);
        var memberRepository = CreateMockMemberRepository(new[] { CreateMember(id: currentMemberId), CreateMember(id: memberId) });
        var subscription = CreateMemberSiteSubscription(feature: SiteFeatureType.AdminMembers);
        var memberSubscriptionRepository = CreateMockMemberSiteSubscriptionRepository(subscription);
        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberSiteSubscriptionRepository: memberSubscriptionRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act
        var result = await service.AddChapterAdminMember(request, memberId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Member is already a chapter admin");
    }

    [Test]
    public static async Task AddChapterAdminMember_WhenValid_ReturnsSuccess()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var currentMemberId = Guid.NewGuid();
        var chapter = CreateChapter();
        var adminMembers = new List<ChapterAdminMember> { CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id) };
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(adminMembers);
        var memberRepository = CreateMockMemberRepository(new[] { CreateMember(id: currentMemberId), CreateMember(id: memberId) });
        var subscription = CreateMemberSiteSubscription(feature: SiteFeatureType.AdminMembers);
        var memberSubscriptionRepository = CreateMockMemberSiteSubscriptionRepository(subscription);
        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository,
            memberSiteSubscriptionRepository: memberSubscriptionRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act
        var result = await service.AddChapterAdminMember(request, memberId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task CreateChapter_WhenChapterLimitReached_ReturnsFailure()
    {
        // Arrange
        var currentMemberId = Guid.NewGuid();
        var subscription = CreateMemberSiteSubscription(groupLimit: 1);
        var existingChapters = new List<Chapter> { CreateChapter(ownerId: currentMemberId) };
        var memberSubscriptionRepository = CreateMockMemberSiteSubscriptionRepository(memberSubscription: subscription);
        var chapterRepository = CreateMockChapterRepository(existingChapters);
        var siteEmailSettingsRepository = CreateMockSiteEmailSettingsRepository(new SiteEmailSettings());
        var unitOfWork = CreateMockUnitOfWork(
            memberSiteSubscriptionRepository: memberSubscriptionRepository,
            chapterRepository: chapterRepository,
            siteEmailSettingsRepository: siteEmailSettingsRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberServiceRequest(currentMemberId: currentMemberId);
        var model = CreateChapterCreateModel();

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("You cannot create any more groups");
    }

    [Test]
    public static async Task CreateChapter_WhenSubscriptionExpired_ReturnsFailure()
    {
        // Arrange
        var currentMemberId = Guid.NewGuid();
        var subscription = CreateMemberSiteSubscription(expiresUtc: DateTime.UtcNow.AddDays(-1));
        var memberSubscriptionRepository = CreateMockMemberSiteSubscriptionRepository(memberSubscription: subscription);
        var chapterRepository = CreateMockChapterRepository(new List<Chapter>());
        var siteEmailSettingsRepository = CreateMockSiteEmailSettingsRepository(new SiteEmailSettings());
        var unitOfWork = CreateMockUnitOfWork(
            memberSiteSubscriptionRepository: memberSubscriptionRepository,
            chapterRepository: chapterRepository,
            siteEmailSettingsRepository: siteEmailSettingsRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberServiceRequest(currentMemberId: currentMemberId);
        var model = CreateChapterCreateModel();

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Your subscription has expired");
    }

    [Test]
    public static async Task CreateChapter_WhenNameTaken_ReturnsFailure()
    {
        // Arrange
        var currentMemberId = Guid.NewGuid();
        var subscription = CreateMemberSiteSubscription();
        var existingChapters = new List<Chapter> { CreateChapter(name: "Test Chapter") };
        var memberSubscriptionRepository = CreateMockMemberSiteSubscriptionRepository(memberSubscription: subscription);
        var chapterRepository = CreateMockChapterRepository(existingChapters);
        var siteEmailSettingsRepository = CreateMockSiteEmailSettingsRepository(new SiteEmailSettings());
        var unitOfWork = CreateMockUnitOfWork(
            memberSiteSubscriptionRepository: memberSubscriptionRepository,
            chapterRepository: chapterRepository,
            siteEmailSettingsRepository: siteEmailSettingsRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberServiceRequest(currentMemberId: currentMemberId);
        var model = CreateChapterCreateModel();

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("is taken");
    }

    [Test]
    public static async Task CreateChapter_WhenImageInvalid_ReturnsFailure()
    {
        // Arrange
        var currentMemberId = Guid.NewGuid();
        var imageData = new byte[] { 1, 2, 3 };
        var subscription = CreateMemberSiteSubscription();
        var memberSubscriptionRepository = CreateMockMemberSiteSubscriptionRepository(memberSubscription: subscription);
        var chapterRepository = CreateMockChapterRepository(new List<Chapter>());
        var siteEmailSettingsRepository = CreateMockSiteEmailSettingsRepository(new SiteEmailSettings());
        var imageService = CreateMockImageService(isValidImage: false);
        var unitOfWork = CreateMockUnitOfWork(
            memberSiteSubscriptionRepository: memberSubscriptionRepository,
            chapterRepository: chapterRepository,
            siteEmailSettingsRepository: siteEmailSettingsRepository);
        var service = CreateChapterAdminService(unitOfWork, imageService: imageService);
        var request = CreateMemberServiceRequest(currentMemberId: currentMemberId);
        var model = CreateChapterCreateModel(imageData: imageData);

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid image");
    }

    [Test]
    public static async Task CreateChapter_WhenValid_ReturnsSuccessfulChapter()
    {
        // Arrange
        var currentMemberId = Guid.NewGuid();
        var imageData = new byte[] { 1, 2, 3 };
        var subscription = CreateMemberSiteSubscription();
        var memberSubscriptionRepository = CreateMockMemberSiteSubscriptionRepository(memberSubscription: subscription);
        var chapterRepository = CreateMockChapterRepository(new List<Chapter>());
        var siteEmailSettingsRepository = CreateMockSiteEmailSettingsRepository(new SiteEmailSettings());
        var imageService = CreateMockImageService(isValidImage: true, processedData: imageData);
        var htmlSanitizer = CreateMockHtmlSanitizer();
        var geolocationService = CreateMockGeolocationService();
        var topicService = CreateMockTopicService();
        var memberEmailService = CreateMockMemberEmailService();
        var unitOfWork = CreateMockUnitOfWork(
            memberSiteSubscriptionRepository: memberSubscriptionRepository,
            chapterRepository: chapterRepository,
            siteEmailSettingsRepository: siteEmailSettingsRepository);
        var service = CreateChapterAdminService(
            unitOfWork,
            imageService: imageService,
            htmlSanitizer: htmlSanitizer,
            geolocationService: geolocationService,
            topicService: topicService,
            memberEmailService: memberEmailService);
        var request = CreateMemberServiceRequest(currentMemberId: currentMemberId);
        var model = CreateChapterCreateModel(imageData: imageData);

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Test Chapter");
    }

    [Test]
    public static async Task DeleteChapterAdminMember_WhenNotFound_ReturnsFailure()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var currentMemberId = Guid.NewGuid();
        var chapter = CreateChapter();
        var adminMembers = new List<ChapterAdminMember> { CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id) };
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(adminMembers);
        var memberRepository = CreateMockMemberRepository(new[] { CreateMember(id: memberId) });
        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act
        var result = await service.DeleteChapterAdminMember(request, memberId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Admin member not found");
    }

    [Test]
    public static async Task DeleteChapterAdminMember_WhenDeletingSiteAdmin_ReturnsFailure()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var currentMemberId = Guid.NewGuid();
        var chapter = CreateChapter();
        var adminMembers = new List<ChapterAdminMember>
        {
            CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id),
            CreateChapterAdminMember(currentMemberId: memberId, chapterId: chapter.Id)
        };
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(adminMembers);
        var memberRepository = CreateMockMemberRepository(new[] { CreateMember(id: memberId, siteAdmin: true) });
        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act
        var result = await service.DeleteChapterAdminMember(request, memberId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot delete a site admin");
    }

    [Test]
    public static async Task DeleteChapterAdminMember_WhenDeletingOwner_ReturnsFailure()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var currentMemberId = Guid.NewGuid();
        var chapter = CreateChapter();
        var adminMembers = new List<ChapterAdminMember>
        {
            CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id),
            CreateChapterAdminMember(currentMemberId: memberId, chapterId: chapter.Id)
        };
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(adminMembers);
        var memberRepository = CreateMockMemberRepository(new[] { CreateMember(id: memberId) });
        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);
        request.Chapter.OwnerId = memberId;

        // Act
        var result = await service.DeleteChapterAdminMember(request, memberId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot delete owner");
    }

    [Test]
    public static async Task DeleteChapterAdminMember_WhenValid_ReturnsSuccess()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var currentMemberId = Guid.NewGuid();
        var chapter = CreateChapter();
        var adminMembers = new List<ChapterAdminMember>
        {
            CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id),
            CreateChapterAdminMember(currentMemberId: memberId, chapterId: chapter.Id)
        };
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(adminMembers);
        var memberRepository = CreateMockMemberRepository(new[] { CreateMember(id: memberId) });
        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: chapterAdminMemberRepository,
            memberRepository: memberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act
        var result = await service.DeleteChapterAdminMember(request, memberId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task DeleteChapterContactMessage_WhenMessageDeleted_ReturnsSuccess()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var chapter = CreateChapter();
        var currentMemberId = Guid.NewGuid();
        var message = CreateChapterContactMessage(id: messageId, chapterId: chapter.Id);
        var contactMessageRepository = CreateMockChapterContactMessageRepository(message);
        var adminMember = CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(new[] { adminMember });
        var unitOfWork = CreateMockUnitOfWork(
            chapterContactMessageRepository: contactMessageRepository,
            chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act
        var result = await service.DeleteChapterContactMessage(request, messageId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task CreateChapterProperty_WhenValid_ReturnsSuccess()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMemberId = Guid.NewGuid();
        var adminMember = CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(new[] { adminMember });
        var propertyRepository = CreateMockChapterPropertyRepository(new List<ChapterProperty>());
        var unitOfWork = CreateMockUnitOfWork(
            chapterPropertyRepository: propertyRepository,
            chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);
        var model = CreateChapterPropertyCreateModel();

        // Act
        var result = await service.CreateChapterProperty(request, model);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task CreateChapterProperty_WithDropDownOptions_AddsOptions()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMemberId = Guid.NewGuid();
        var propertyRepository = CreateMockChapterPropertyRepository(new List<ChapterProperty>());
        var adminMember = CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(new[] { adminMember });
        var unitOfWork = CreateMockUnitOfWork(
            chapterPropertyRepository: propertyRepository,
            chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);
        var model = CreateChapterPropertyCreateModel(dataType: DataType.DropDown, options: new List<string> { "Option 1", "Option 2" });

        // Act
        var result = await service.CreateChapterProperty(request, model);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task CreateChapterProperty_WhenMissingRequiredFields_ReturnsFailure()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMemberId = Guid.NewGuid();
        var propertyRepository = CreateMockChapterPropertyRepository(new List<ChapterProperty>());
        var adminMember = CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(new[] { adminMember });
        var unitOfWork = CreateMockUnitOfWork(
            chapterPropertyRepository: propertyRepository, 
            chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);
        var model = CreateChapterPropertyCreateModel(label: string.Empty);

        // Act
        var result = await service.CreateChapterProperty(request, model);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public static async Task CreateChapterQuestion_WhenValid_ReturnsSuccess()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMemberId = Guid.NewGuid();
        var questionRepository = CreateMockChapterQuestionRepository(new List<ChapterQuestion>());
        var htmlSanitizer = CreateMockHtmlSanitizer();
        var adminMember = CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(new[] { adminMember });
        var unitOfWork = CreateMockUnitOfWork(
            chapterQuestionRepository: questionRepository,
            chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork, htmlSanitizer: htmlSanitizer);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);
        var model = CreateChapterQuestionCreateModel();

        // Act
        var result = await service.CreateChapterQuestion(request, model);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task CreateChapterQuestion_WhenMissingFields_ReturnsFailure()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMemberId = Guid.NewGuid();
        var questionRepository = CreateMockChapterQuestionRepository(new List<ChapterQuestion>());
        var adminMember = CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(new[] { adminMember });
        var unitOfWork = CreateMockUnitOfWork(
            chapterQuestionRepository: questionRepository,
            chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);
        var model = CreateChapterQuestionCreateModel(name: string.Empty);

        // Act
        var result = await service.CreateChapterQuestion(request, model);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public static async Task GetChapterAdminPageViewModel_ReturnsViewModel()
    {
        // Arrange
        var chapter = CreateChapter(name: "Test Chapter");
        var currentMemberId = Guid.NewGuid();
        var chapterRepository = CreateMockChapterRepository(new[] { chapter });
        var adminMember = CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(new[] { adminMember });
        var unitOfWork = CreateMockUnitOfWork(
            chapterRepository: chapterRepository,
            chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act
        var result = await service.GetChapterAdminPageViewModel(request);

        // Assert
        result.Should().NotBeNull();
        result.Chapter.Should().Be(chapter);
    }

    [Test]
    public static async Task GetChapterDeleteViewModel_ReturnsViewModelWithMemberCount()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMemberId = Guid.NewGuid();
        var memberCount = 42;
        var memberRepository = CreateMockMemberRepository(memberCount: memberCount);
        var adminMember = CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(new[] { adminMember });
        var unitOfWork = CreateMockUnitOfWork(
            memberRepository: memberRepository,
            chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act
        var result = await service.GetChapterDeleteViewModel(request);

        // Assert
        result.Should().NotBeNull();
        result.MemberCount.Should().Be(memberCount);
    }

    [Test]
    public static async Task GetChapterLinksViewModel_ReturnsViewModel()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMemberId = Guid.NewGuid();
        var links = CreateChapterLinks(chapterId: chapter.Id);
        var subscription = CreateMemberSiteSubscription(feature: SiteFeatureType.AdminMembers);
        var privacySettings = CreateChapterPrivacySettings(chapterId: chapter.Id);
        var memberSubscriptionRepository = CreateMockMemberSiteSubscriptionRepository(subscription);
        var linksRepository = CreateMockChapterLinksRepository(links);
        var privacyRepository = CreateMockChapterPrivacySettingsRepository(privacySettings);
        var adminMember = CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(new[] { adminMember });
        var unitOfWork = CreateMockUnitOfWork(
            memberSiteSubscriptionRepository: memberSubscriptionRepository,
            chapterLinksRepository: linksRepository,
            chapterPrivacySettingsRepository: privacyRepository,
            chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act
        var result = await service.GetChapterLinksViewModel(request);

        // Assert
        result.Should().NotBeNull();
        result.Links.Should().Be(links);
    }

    [Test]
    public static async Task GetChapterPropertiesViewModel_ReturnsProperties()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMemberId = Guid.NewGuid();
        var properties = new List<ChapterProperty> { CreateChapterProperty(chapterId: chapter.Id, name: "prop1") };
        var propertyRepository = CreateMockChapterPropertyRepository(properties);
        var adminMember = CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(new[] { adminMember });
        var unitOfWork = CreateMockUnitOfWork(
            chapterPropertyRepository: propertyRepository,
            chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act
        var result = await service.GetChapterPropertiesViewModel(request);

        // Assert
        result.Should().NotBeNull();
        result.Properties.Should().HaveCount(1);
        result.Properties.First().Name.Should().Be("prop1");
    }

    [Test]
    public static async Task GetChapterQuestionsViewModel_ReturnsQuestions()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMemberId = Guid.NewGuid();
        var questions = new List<ChapterQuestion> { CreateChapterQuestion(chapterId: chapter.Id, name: "q1") };
        var questionRepository = CreateMockChapterQuestionRepository(questions);
        var adminMember = CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(new[] { adminMember });
        var unitOfWork = CreateMockUnitOfWork(
            chapterQuestionRepository: questionRepository,
            chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act
        var result = await service.GetChapterQuestionsViewModel(request);

        // Assert
        result.Should().NotBeNull();
        result.Questions.Should().HaveCount(1);
        result.Questions.First().Name.Should().Be("q1");
    }

    [Test]
    public static async Task UpdateChapterAdminMember_WhenValid_UpdatesSuccessfully()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var currentMemberId = Guid.NewGuid();
        var chapter = CreateChapter();
        var adminMembers = new List<ChapterAdminMember>
        {
            CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id),
            CreateChapterAdminMember(currentMemberId: memberId, chapterId: chapter.Id)
        };
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(adminMembers);
        var unitOfWork = CreateMockUnitOfWork(chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);
        var model = CreateChapterAdminMemberUpdateModel();

        // Act
        var result = await service.UpdateChapterAdminMember(request, memberId, model);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateChapterImage_WhenInvalidImage_ReturnsFailure()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMemberId = Guid.NewGuid();
        var imageData = new byte[] { 1, 2, 3 };
        var image = CreateChapterImage(chapterId: chapter.Id);
        var imageRepository = CreateMockChapterImageRepository(image);
        var imageService = CreateMockImageService(isValidImage: false);
        var adminMember = CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(new[] { adminMember });
        var unitOfWork = CreateMockUnitOfWork(
            chapterImageRepository: imageRepository,
            chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork, imageService: imageService);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);
        var model = CreateChapterImageUpdateModel(imageData: imageData);

        // Act
        var result = await service.UpdateChapterImage(request, model);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid image");
    }

    [Test]
    public static async Task UpdateChapterImage_WhenValidImage_UpdatesSuccessfully()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMemberId = Guid.NewGuid();
        var imageData = new byte[] { 1, 2, 3 };
        var image = CreateChapterImage(chapterId: chapter.Id);
        var imageRepository = CreateMockChapterImageRepository(image);
        var imageService = CreateMockImageService(isValidImage: true, processedData: imageData);
        var adminMember = CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(new[] { adminMember });
        var unitOfWork = CreateMockUnitOfWork(
            chapterImageRepository: imageRepository,
            chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork, imageService: imageService);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);
        var model = CreateChapterImageUpdateModel(imageData: imageData);

        // Act
        var result = await service.UpdateChapterImage(request, model);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateChapterDescription_UpdatesSuccessfully()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMemberId = Guid.NewGuid();
        var description = "<p>New Description</p>";
        var texts = CreateChapterTexts(chapterId: chapter.Id);
        var textsRepository = CreateMockChapterTextsRepository(texts);
        var htmlSanitizer = CreateMockHtmlSanitizer();
        var adminMember = CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id);
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(new[] { adminMember });
        var unitOfWork = CreateMockUnitOfWork(
            chapterTextsRepository: textsRepository, 
            chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork, htmlSanitizer: htmlSanitizer);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act
        var result = await service.UpdateChapterDescription(request, description);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateChapterTheme_UpdatesSuccessfully()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMemberId = Guid.NewGuid();
        var adminMembers = new List<ChapterAdminMember> { CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id) };
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(adminMembers);
        var unitOfWork = CreateMockUnitOfWork(chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);
        var model = CreateChapterThemeUpdateModel();

        // Act
        var result = await service.UpdateChapterTheme(request, model);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task PublishChapter_WhenCanBePublished_PublishesSuccessfully()
    {
        // Arrange
        var chapter = CreateChapter(approvedUtc: DateTime.UtcNow);
        var currentMemberId = Guid.NewGuid();
        var adminMembers = new List<ChapterAdminMember> { CreateChapterAdminMember(currentMemberId: currentMemberId, chapterId: chapter.Id) };
        var chapterAdminMemberRepository = CreateMockChapterAdminMemberRepository(adminMembers);
        var unitOfWork = CreateMockUnitOfWork(chapterAdminMemberRepository: chapterAdminMemberRepository);
        var service = CreateChapterAdminService(unitOfWork);
        var request = CreateMemberChapterAdminServiceRequest(chapter: chapter, currentMemberId: currentMemberId);

        // Act
        var result = await service.PublishChapter(request);

        // Assert
        result.Success.Should().BeTrue();
    }

    private static IChapterAdminMemberRepository CreateMockChapterAdminMemberRepository(IEnumerable<ChapterAdminMember>? adminMembers = null)
    {
        var mock = new Mock<IChapterAdminMemberRepository>();

        adminMembers ??= new List<ChapterAdminMember>();

        mock.Setup(x => x.GetByMemberId(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns((Guid memberId, Guid chapterId) =>
                new MockDeferredQuerySingle<ChapterAdminMember>(
                    adminMembers.FirstOrDefault(x => x.ChapterId == chapterId && x.MemberId == memberId)));

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns((Guid chapterId) =>
                new MockDeferredQueryMultiple<ChapterAdminMember>(
                    adminMembers.Where(x => x.ChapterId == chapterId)));

        mock.Setup(x => x.Add(It.IsAny<ChapterAdminMember>()));
        mock.Setup(x => x.Update(It.IsAny<ChapterAdminMember>()));
        mock.Setup(x => x.Delete(It.IsAny<ChapterAdminMember>()));

        return mock.Object;
    }

    private static IMemberRepository CreateMockMemberRepository(IEnumerable<Member>? members = null, int? memberCount = null)
    {
        var mock = new Mock<IMemberRepository>();
        members ??= new List<Member>();

        mock.Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns((Guid memberId) =>
                new MockDeferredQuerySingle<Member>(
                    members.FirstOrDefault(x => x.Id == memberId)));

        if (memberCount.HasValue)
        {
            mock.Setup(x => x.GetCountByChapterId(It.IsAny<Guid>()))
                .Returns(new MockDeferredQuery<int>(memberCount.Value));
        }

        return mock.Object;
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

    private static IChapterRepository CreateMockChapterRepository(IEnumerable<Chapter>? chapters = null)
    {
        var mock = new Mock<IChapterRepository>();
        chapters ??= new List<Chapter>();

        mock.Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns((Guid chapterId) =>
                new MockDeferredQuerySingle<Chapter>(
                    chapters.FirstOrDefault(x => x.Id == chapterId)!));

        mock.Setup(x => x.GetAll())
            .Returns(new MockDeferredQueryMultiple<Chapter>(chapters));

        mock.Setup(x => x.Add(It.IsAny<Chapter>()));
        mock.Setup(x => x.Update(It.IsAny<Chapter>()));

        return mock.Object;
    }

    private static ISiteEmailSettingsRepository CreateMockSiteEmailSettingsRepository(SiteEmailSettings? settings = null)
    {
        var mock = new Mock<ISiteEmailSettingsRepository>();

        mock.Setup(x => x.Get(It.IsAny<PlatformType>()))
            .Returns(new MockDeferredQuerySingle<SiteEmailSettings>(
                settings ?? new SiteEmailSettings()));

        return mock.Object;
    }

    private static IChapterContactMessageRepository CreateMockChapterContactMessageRepository(ChapterContactMessage? message = null)
    {
        var mock = new Mock<IChapterContactMessageRepository>();

        mock.Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingle<ChapterContactMessage>(message));

        mock.Setup(x => x.Delete(It.IsAny<ChapterContactMessage>()));

        return mock.Object;
    }

    private static IChapterPropertyRepository CreateMockChapterPropertyRepository(IEnumerable<ChapterProperty>? properties = null)
    {
        var mock = new Mock<IChapterPropertyRepository>();
        properties ??= new List<ChapterProperty>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns((Guid chapterId) =>
                new MockDeferredQueryMultiple<ChapterProperty>(
                    properties.Where(x => x.ChapterId == chapterId)));

        mock.Setup(x => x.Add(It.IsAny<ChapterProperty>()));

        return mock.Object;
    }

    private static IChapterQuestionRepository CreateMockChapterQuestionRepository(IEnumerable<ChapterQuestion>? questions = null)
    {
        var mock = new Mock<IChapterQuestionRepository>();
        questions ??= new List<ChapterQuestion>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns((Guid chapterId) =>
                new MockDeferredQueryMultiple<ChapterQuestion>(
                    questions.Where(x => x.ChapterId == chapterId)));

        mock.Setup(x => x.Add(It.IsAny<ChapterQuestion>()));

        return mock.Object;
    }

    private static IChapterLinksRepository CreateMockChapterLinksRepository(ChapterLinks? links = null)
    {
        var mock = new Mock<IChapterLinksRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingleOrDefault<ChapterLinks>(links));

        return mock.Object;
    }

    private static IChapterPrivacySettingsRepository CreateMockChapterPrivacySettingsRepository(ChapterPrivacySettings? settings = null)
    {
        var mock = new Mock<IChapterPrivacySettingsRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingleOrDefault<ChapterPrivacySettings>(settings));

        return mock.Object;
    }

    private static IChapterImageRepository CreateMockChapterImageRepository(ChapterImage? image = null)
    {
        var mock = new Mock<IChapterImageRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingleOrDefault<ChapterImage>(image));

        mock.Setup(x => x.Upsert(It.IsAny<ChapterImage>(), It.IsAny<Guid>()));

        return mock.Object;
    }

    private static IChapterTextsRepository CreateMockChapterTextsRepository(ChapterTexts? texts = null)
    {
        var mock = new Mock<IChapterTextsRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingleOrDefault<ChapterTexts>(texts));

        mock.Setup(x => x.Add(It.IsAny<ChapterTexts>()));
        mock.Setup(x => x.Update(It.IsAny<ChapterTexts>()));

        return mock.Object;
    }

    private static MockUnitOfWork CreateMockUnitOfWork(
        IChapterAdminMemberRepository? chapterAdminMemberRepository = null,
        IMemberRepository? memberRepository = null,
        IMemberSiteSubscriptionRepository? memberSiteSubscriptionRepository = null,
        IChapterRepository? chapterRepository = null,
        ISiteEmailSettingsRepository? siteEmailSettingsRepository = null,
        IChapterContactMessageRepository? chapterContactMessageRepository = null,
        IChapterPropertyRepository? chapterPropertyRepository = null,
        IChapterQuestionRepository? chapterQuestionRepository = null,
        IChapterLinksRepository? chapterLinksRepository = null,
        IChapterPrivacySettingsRepository? chapterPrivacySettingsRepository = null,
        IChapterImageRepository? chapterImageRepository = null,
        IChapterTextsRepository? chapterTextsRepository = null)
    {
        var mock = new Mock<IUnitOfWork>();

        mock.Setup(x => x.ChapterAdminMemberRepository)
            .Returns(chapterAdminMemberRepository ?? CreateMockChapterAdminMemberRepository());
        mock.Setup(x => x.MemberRepository)
            .Returns(memberRepository ?? CreateMockMemberRepository());
        mock.Setup(x => x.MemberSiteSubscriptionRepository)
            .Returns(memberSiteSubscriptionRepository ?? CreateMockMemberSiteSubscriptionRepository());
        mock.Setup(x => x.ChapterRepository)
            .Returns(chapterRepository ?? CreateMockChapterRepository());
        mock.Setup(x => x.SiteEmailSettingsRepository)
            .Returns(siteEmailSettingsRepository ?? CreateMockSiteEmailSettingsRepository());
        mock.Setup(x => x.ChapterContactMessageRepository)
            .Returns(chapterContactMessageRepository ?? CreateMockChapterContactMessageRepository());
        mock.Setup(x => x.ChapterPropertyRepository)
            .Returns(chapterPropertyRepository ?? CreateMockChapterPropertyRepository());
        mock.Setup(x => x.ChapterQuestionRepository)
            .Returns(chapterQuestionRepository ?? CreateMockChapterQuestionRepository());
        mock.Setup(x => x.ChapterLinksRepository)
            .Returns(chapterLinksRepository ?? CreateMockChapterLinksRepository());
        mock.Setup(x => x.ChapterPrivacySettingsRepository)
            .Returns(chapterPrivacySettingsRepository ?? CreateMockChapterPrivacySettingsRepository());
        mock.Setup(x => x.ChapterImageRepository)
            .Returns(chapterImageRepository ?? CreateMockChapterImageRepository());
        mock.Setup(x => x.ChapterTextsRepository)
            .Returns(chapterTextsRepository ?? CreateMockChapterTextsRepository());

        mock.Setup(x => x.ChapterLocationRepository.Add(It.IsAny<ChapterLocation>()));
        mock.Setup(x => x.MemberChapterRepository.Add(It.IsAny<MemberChapter>()));
        mock.Setup(x => x.ChapterTopicRepository.AddMany(It.IsAny<IEnumerable<ChapterTopic>>()));
        mock.Setup(x => x.ChapterPaymentSettingsRepository.Add(It.IsAny<ChapterPaymentSettings>()));
        mock.Setup(x => x.ChapterPropertyOptionRepository.AddMany(It.IsAny<IEnumerable<ChapterPropertyOption>>()));

        return new MockUnitOfWork(mock);
    }

    private static IImageService CreateMockImageService(bool isValidImage, byte[]? processedData = null)
    {
        var mock = new Mock<IImageService>();
        mock.Setup(x => x.IsImage(It.IsAny<byte[]>())).Returns(isValidImage);
        if (processedData != null)
            mock.Setup(x => x.Process(It.IsAny<byte[]>(), It.IsAny<ImageProcessingOptions>())).Returns(processedData);
        return mock.Object;
    }

    private static IHtmlSanitizer CreateMockHtmlSanitizer()
    {
        var mock = new Mock<IHtmlSanitizer>();
        mock.Setup(x => x.Sanitize(It.IsAny<string>(), It.IsAny<HtmlSanitizerOptions>()))
            .Returns((string html, HtmlSanitizerOptions _) => html);
        return mock.Object;
    }

    private static IGeolocationService CreateMockGeolocationService()
    {
        var mock = new Mock<IGeolocationService>();
        mock.Setup(x => x.GetTimeZoneFromLocation(It.IsAny<LatLong>()))
            .ReturnsAsync(TimeZoneInfo.FindSystemTimeZoneById("Europe/London"));

        mock.Setup(x => x.GetCountryFromLocation(It.IsAny<LatLong>()))
            .ReturnsAsync(CreateCountry());
        return mock.Object;
    }

    private static ITopicService CreateMockTopicService()
    {
        var mock = new Mock<ITopicService>();
        mock.Setup(x => x.AddNewChapterTopics(It.IsAny<MemberChapterServiceRequest>(), It.IsAny<IReadOnlyCollection<NewTopicModel>>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    private static IMemberEmailService CreateMockMemberEmailService()
    {
        var mock = new Mock<IMemberEmailService>();
        mock.Setup(x => x.SendNewGroupEmail(It.IsAny<MemberServiceRequest>(), It.IsAny<Chapter>(), It.IsAny<ChapterTexts>(), It.IsAny<SiteEmailSettings>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    private static ChapterAdminService CreateChapterAdminService(
        MockUnitOfWork unitOfWork,
        ICacheService? cacheService = null,
        IHtmlSanitizer? htmlSanitizer = null,
        ISocialMediaService? socialMediaService = null,
        INotificationService? notificationService = null,
        IImageService? imageService = null,
        IMemberEmailService? memberEmailService = null,
        ITopicService? topicService = null,
        ISiteSubscriptionService? siteSubscriptionService = null,
        IUrlProviderFactory? urlProviderFactory = null,
        IPaymentProviderFactory? paymentProviderFactory = null,
        IPaymentService? paymentService = null,
        IGeolocationService? geolocationService = null,
        ILoggingService? loggingService = null)
    {
        var settings = new ChapterAdminServiceSettings { ContactMessageRecaptchaScoreThreshold = 0.5 };
        return new ChapterAdminService(
            unitOfWork,
            cacheService ?? new Mock<ICacheService>().Object,
            htmlSanitizer ?? new Mock<IHtmlSanitizer>().Object,
            socialMediaService ?? new Mock<ISocialMediaService>().Object,
            notificationService ?? new Mock<INotificationService>().Object,
            imageService ?? new Mock<IImageService>().Object,
            memberEmailService ?? new Mock<IMemberEmailService>().Object,
            topicService ?? new Mock<ITopicService>().Object,
            settings,
            siteSubscriptionService ?? new Mock<ISiteSubscriptionService>().Object,
            urlProviderFactory ?? new Mock<IUrlProviderFactory>().Object,
            paymentProviderFactory ?? new Mock<IPaymentProviderFactory>().Object,
            paymentService ?? new Mock<IPaymentService>().Object,
            geolocationService ?? new Mock<IGeolocationService>().Object,
            loggingService ?? new Mock<ILoggingService>().Object);
    }

    private static MemberChapterAdminServiceRequest CreateMemberChapterAdminServiceRequest(
        Chapter? chapter = null,
        Guid? currentMemberId = null,
        PlatformType? platform = null)
    {
        var currentMember = CreateMember(id: currentMemberId);

        return new MemberChapterAdminServiceRequest
        {
            Chapter = chapter ?? CreateChapter(),
            CurrentMemberIdOrDefault = currentMember.Id,
            CurrentMember = currentMember,
            Platform = platform ?? PlatformType.Default,
            HttpRequestContext = CreateHttpRequestContext(),
            Securable = ChapterAdminSecurable.Any
        };
    }

    private static MemberServiceRequest CreateMemberServiceRequest(Guid? currentMemberId = null, PlatformType? platform = null)
    {
        var currentMember = CreateMember(id: currentMemberId);

        return new MemberServiceRequest
        {
            CurrentMemberIdOrDefault = currentMember.Id,
            CurrentMember = currentMember,
            Platform = platform ?? PlatformType.Default,
            HttpRequestContext = CreateHttpRequestContext()
        };
    }

    private static IHttpRequestContext CreateHttpRequestContext(string? baseUrl = null)
    {
        var mock = new Mock<IHttpRequestContext>();

        mock.Setup(m => m.BaseUrl)
            .Returns(baseUrl ?? "https://test.local");

        return mock.Object;
    }

    private static ChapterCreateModel CreateChapterCreateModel(
        string? name = null,
        string? description = null,
        string? locationName = null,
        LatLong? location = null,
        byte[]? imageData = null,
        List<Guid>? topicIds = null)
        => new ChapterCreateModel
        {
            Name = name ?? "Test Chapter",
            Description = description ?? "<p>Test</p>",
            LocationName = locationName ?? "London",
            Location = location ?? new LatLong { Lat = 51.5, Long = -0.1 },
            ImageData = imageData ?? Array.Empty<byte>(),
            NewTopics = new List<NewTopicModel>(),
            TopicIds = topicIds ?? new List<Guid> { Guid.NewGuid() }
        };

    private static ChapterProperty CreateChapterProperty(
        Guid? chapterId = null,
        string? displayName = null,
        string? label = null,
        string? name = null,
        bool? required = null,
        DataType? dataType = null)
        => new ChapterProperty
        {
            ChapterId = chapterId ?? Guid.NewGuid(),
            DisplayName = displayName ?? "Test Property",
            Label = label ?? "test-property",
            Name = name ?? "test-property",
            Required = required ?? true,
            DataType = dataType ?? DataType.Text
        };

    private static ChapterPropertyCreateModel CreateChapterPropertyCreateModel(
        string? displayName = null,
        string? label = null,
        string? name = null,
        bool? required = null,
        DataType? dataType = null,
        List<string>? options = null)
        => new ChapterPropertyCreateModel
        {
            DisplayName = displayName ?? "Test Property",
            Label = label ?? "test-property",
            Name = name ?? "test-property",
            Required = required ?? true,
            DataType = dataType ?? DataType.Text,
            Options = options
        };

    private static ChapterQuestion CreateChapterQuestion(
        Guid? chapterId = null,
        string? name = null,
        string? answer = null)
        => new ChapterQuestion
        {
            ChapterId = chapterId ?? Guid.NewGuid(),
            Name = name ?? "Test Question",
            Answer = answer ?? "<p>Test Answer</p>"
        };

    private static ChapterQuestionCreateModel CreateChapterQuestionCreateModel(
        string? name = null,
        string? answer = null)
        => new ChapterQuestionCreateModel
        {
            Name = name ?? "Test Question",
            Answer = answer ?? "<p>Test Answer</p>"
        };

    private static ChapterAdminMemberUpdateModel CreateChapterAdminMemberUpdateModel(
        string? adminEmailAddress = null,
        bool? receiveContactEmails = null,
        bool? receiveEventCommentEmails = null,
        bool? receiveNewMemberEmails = null)
        => new ChapterAdminMemberUpdateModel
        {
            AdminEmailAddress = adminEmailAddress ?? "admin@test.com",
            ReceiveContactEmails = receiveContactEmails ?? true,
            ReceiveEventCommentEmails = receiveEventCommentEmails ?? true,
            ReceiveNewMemberEmails = receiveNewMemberEmails ?? true
        };

    private static ChapterImageUpdateModel CreateChapterImageUpdateModel(byte[]? imageData = null)
        => new ChapterImageUpdateModel { ImageData = imageData ?? new byte[] { 1, 2, 3 } };

    private static ChapterThemeUpdateModel CreateChapterThemeUpdateModel(
        string? background = null,
        string? color = null)
        => new ChapterThemeUpdateModel
        {
            Background = background ?? "#ffffff",
            Color = color ?? "#000000"
        };

    private static Chapter CreateChapter(
        Guid? id = null,
        string? name = null,
        Guid? ownerId = null,
        DateTime? approvedUtc = null)
    {
        var slugName = name ?? "Test Chapter";
        return new Chapter
        {
            ApprovedUtc = approvedUtc,
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
        bool? siteAdmin = null)
        => new Member
        {
            Id = id ?? Guid.NewGuid(),
            SiteAdmin = siteAdmin ?? false
        };

    private static ChapterAdminMember CreateChapterAdminMember(
        Guid? currentMemberId = null,
        Guid? chapterId = null,
        ChapterAdminRole? role = null)
        => new ChapterAdminMember
        {
            Id = Guid.NewGuid(),
            MemberId = currentMemberId ?? Guid.NewGuid(),
            ChapterId = chapterId ?? Guid.NewGuid(),
            Role = role ?? ChapterAdminRole.Admin,
            ReceiveContactEmails = false,
            ReceiveEventCommentEmails = false,
            ReceiveNewMemberEmails = false
        };

    private static ChapterContactMessage CreateChapterContactMessage(
        Guid? id = null,
        Guid? chapterId = null)
        => new ChapterContactMessage
        {
            FromAddress = "",
            Message = "",
            Id = id ?? Guid.NewGuid(),
            ChapterId = chapterId ?? Guid.NewGuid()
        };

    private static ChapterImage CreateChapterImage(Guid? chapterId = null)
        => new ChapterImage { ChapterId = chapterId ?? Guid.NewGuid() };

    private static ChapterTexts CreateChapterTexts(Guid? chapterId = null)
        => new ChapterTexts
        {
            ChapterId = chapterId ?? Guid.NewGuid(),
            Description = "Test description",
            WelcomeText = "Welcome to the test chapter",
            RegisterText = "Register here"
        };

    private static ChapterLinks CreateChapterLinks(Guid? chapterId = null)
        => new ChapterLinks
        {
            ChapterId = chapterId ?? Guid.NewGuid(),
            FacebookName = null,
            InstagramName = null,
            TwitterName = null,
            Version = []
        };

    private static ChapterPrivacySettings CreateChapterPrivacySettings(Guid? chapterId = null)
        => new ChapterPrivacySettings { ChapterId = chapterId ?? Guid.NewGuid() };

    private static Country CreateCountry() => new Country
    {
        Continent = "",
        Id = Guid.NewGuid(),
        IsoCode2 = "GB",
        IsoCode3 = "",
        Name = ""
    };

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
                ? new List<SiteSubscriptionFeature> { new SiteSubscriptionFeature { Feature = feature.Value } }
                : new List<SiteSubscriptionFeature>()
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
}
using System;
using System.Collections.Generic;
using System.Data;
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
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Resources.Resources;
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

[Parallelizable]
public static class ChapterAdminServiceTests
{
    [Test]
    public static void AddChapterAdminMember_WhenMemberNotChapterAdmin_ThrowsException()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, member) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            siteSubscription: context.CreateSiteSubscription(
                features: [SiteFeatureType.AdminMembers]));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        // Act & Assert
        Assert.ThrowsAsync<OdkNotAuthorizedException>(
            async () => await service.AddChapterAdminMember(request, member.Id));
    }

    [Test]
    public static async Task AddChapterAdminMember_WhenFeatureNotEnabled_ReturnsFailure()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, member) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(adminMembers: [currentMember]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        // Act
        var result = await service.AddChapterAdminMember(request, member.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Not permitted");
    }

    [Test]
    public static async Task AddChapterAdminMember_WhenMemberAlreadyAdmin_ReturnsFailure()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, otherMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            siteSubscription: context.CreateSiteSubscription(features: [SiteFeatureType.AdminMembers]),
            adminMembers: [currentMember, otherMember]);

        var service = CreateChapterAdminService(context);
        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        // Act
        var result = await service.AddChapterAdminMember(request, otherMember.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Member is already a chapter admin");
    }

    [TestCase(ChapterAdminRole.Owner)]
    [TestCase(ChapterAdminRole.Admin)]
    public static async Task AddChapterAdminMember_WhenMemberHasRole_ReturnsSuccess(ChapterAdminRole role)
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, otherMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            siteSubscription: context.CreateSiteSubscription(features: [SiteFeatureType.AdminMembers]),
            afterCreate: x => context.CreateChapterAdminMember(x, currentMember, role: role));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        // Act
        var result = await service.AddChapterAdminMember(request, otherMember.Id);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task AddChapterAdminMember_SetsOrganiserRole()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, otherMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            siteSubscription: context.CreateSiteSubscription(features: [SiteFeatureType.AdminMembers]),
            adminMembers: [currentMember]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        // Act
        var result = await service.AddChapterAdminMember(request, otherMember.Id);

        // Assert
        var otherAdminMember = context
            .Set<ChapterAdminMember>()
            .Where(x => x.MemberId == otherMember.Id && x.ChapterId == chapter.Id)
            .FirstOrDefault();

        otherAdminMember.Should().NotBeNull();
        otherAdminMember.Role.Should().Be(ChapterAdminRole.Organiser);
    }

    [TestCase(ChapterAdminRole.Organiser)]
    public static async Task AddChapterAdminMember_WhenMemberDoesNotHaveRole_ReturnsSuccess(ChapterAdminRole role)
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, otherMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            siteSubscription: context.CreateSiteSubscription(features: [SiteFeatureType.AdminMembers]),
            afterCreate: x => context.CreateChapterAdminMember(x, currentMember, role: role));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        // Act
        Func<Task> act = () => service.AddChapterAdminMember(request, otherMember.Id);

        // Assert
        await act.Should().ThrowAsync<OdkNotAuthorizedException>();
    }

    [Test]
    public static async Task AddChapterAdminMember_WhenValid_ReturnsSuccess()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, otherMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            siteSubscription: context.CreateSiteSubscription(features: [SiteFeatureType.AdminMembers]),
            adminMembers: [currentMember]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        // Act
        var result = await service.AddChapterAdminMember(request, otherMember.Id);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task CreateChapter_WhenChapterLimitReached_ReturnsFailure()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember(
            afterCreate: x => context.CreateMemberSiteSubscription(
                x, context.CreateSiteSubscription(groupLimit: 1)));

        context.CreateChapter(
            owner: currentMember,
            name: "Existing group");

        var service = CreateChapterAdminService(context);

        var request = CreateMemberServiceRequest(currentMember);
        var model = CreateChapterCreateModel();

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(ErrorMessagesResource.GroupLimitReached);
    }

    [Test]
    public static async Task CreateChapter_WhenSubscriptionExpired_ReturnsFailure()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember(
            afterCreate: x => context.CreateMemberSiteSubscription(
                x, expiresUtc: DateTime.UtcNow.AddDays(-1)));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberServiceRequest(currentMember);
        var model = CreateChapterCreateModel();

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(ErrorMessagesResource.SubscriptionExpired);
    }

    [Test]
    public static async Task CreateChapter_WhenNameTaken_ReturnsFailure()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var name = "Test Chapter";
        context.CreateChapter(name: name);

        var currentMember = context.CreateMember(
            createSiteSubscription: true);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberServiceRequest(currentMember);
        var model = CreateChapterCreateModel(name: name);

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(ErrorMessagesResource.NameTaken.Replace("{name}", name));
    }

    [Test]
    public static async Task CreateChapter_WhenCountryNotFound_UsesDefaultChapter()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var isoCode = "GB";
        var country = context.CreateCountry(isoCode2: isoCode);

        var geolocationService = CreateMockGeolocationService(country: null);

        var settings = CreateChapterAdminServiceSettings(defaultCountryCode: isoCode);

        var service = CreateChapterAdminService(
            context,
            geolocationService: geolocationService,
            settings: settings);

        var request = CreateMemberServiceRequest(currentMember);
        var model = CreateChapterCreateModel();

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Value.Should().NotBeNull();
        result.Value.CountryId.Should().Be(country.Id);
    }

    [Test]
    public static async Task CreateChapter_WhenImageInvalid_ReturnsFailure()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var imageService = CreateMockImageService(isValidImage: false);

        var service = CreateChapterAdminService(context, imageService: imageService);

        var request = CreateMemberServiceRequest(currentMember);
        var model = CreateChapterCreateModel();

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(ErrorMessagesResource.InvalidImage);
    }

    [Test]
    public static async Task CreateChapter_WhenValid_ReturnsSuccessfulChapter()
    {
        // Arrange
        var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var service = CreateChapterAdminService(context);

        var request = CreateMemberServiceRequest(currentMember);
        var model = CreateChapterCreateModel();

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Test Chapter");
        result.Value.Slug.Should().Be("test-chapter");
    }

    [Test]
    public static async Task CreateChapter_WhenSlugExists_AppendsVersion()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        context.CreateChapter(name: "Test Chapter");
        context.CreateChapter(name: "Test Chapter 2");

        var service = CreateChapterAdminService(context);

        var request = CreateMemberServiceRequest(currentMember);
        var model = CreateChapterCreateModel(name: "Test Chapter!");

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Value.Should().NotBeNull();
        result.Value.Slug.Should().Be("test-chapter-3");
    }

    [Test]
    public static async Task DeleteChapterAdminMember_WhenNotFound_ReturnsFailure()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, otherMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        // Act
        var result = await service.DeleteChapterAdminMember(request, otherMember.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Admin member not found");
    }

    [Test]
    public static async Task DeleteChapterAdminMember_WhenDeletingSiteAdmin_ReturnsFailure()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, otherMember) = (context.CreateMember(), context.CreateMember(siteAdmin: true));

        var chapter = context.CreateChapter(
            adminMembers: [currentMember, otherMember]);

        var service = CreateChapterAdminService(context);
        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        // Act
        var result = await service.DeleteChapterAdminMember(request, otherMember.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot delete a site admin");
    }

    [Test]
    public static async Task DeleteChapterAdminMember_WhenDeletingOwner_ReturnsFailure()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, otherMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: otherMember,
            adminMembers: [currentMember]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        // Act
        var result = await service.DeleteChapterAdminMember(request, otherMember.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot delete owner");
    }

    [Test]
    public static async Task DeleteChapterAdminMember_WhenValid_ReturnsSuccess()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, otherMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            adminMembers: [currentMember, otherMember]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        // Act
        var result = await service.DeleteChapterAdminMember(request, otherMember.Id);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task DeleteChapterContactMessage_WhenMessageDeleted_ReturnsSuccess()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        var message = context.Create(CreateChapterContactMessage(chapter: chapter));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.ContactMessages);

        // Act
        var result = await service.DeleteChapterContactMessage(request, message.Id);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task CreateChapterProperty_WhenValid_ReturnsSuccess()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Properties);
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
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Properties);

        var model = CreateChapterPropertyCreateModel(
            dataType: DataType.DropDown, options: ["Option 1", "Option 2"]);

        // Act
        var result = await service.CreateChapterProperty(request, model);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task CreateChapterProperty_WhenMissingRequiredFields_ReturnsFailure()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Properties);

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
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        var unitOfWork = CreateMockUnitOfWork(context);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Questions);

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
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Questions);

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
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        context.Add(currentMember);

        var chapter = context.CreateChapter(
            name: "Test Chapter",
            adminMembers: [currentMember]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Any);

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
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var members = new List<Member>();
        var memberCount = 42;
        for (var i = 1; i <= memberCount; i++)
        {
            members.Add(context.CreateMember());
        }

        var chapter = context.CreateChapter(
            afterCreate: x => context.CreateChapterAdminMember(x, currentMember, role: ChapterAdminRole.Owner),
            members: members);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Delete);

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
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            siteSubscription: context.CreateSiteSubscription(features: [SiteFeatureType.AdminMembers]),
            adminMembers: [currentMember]);

        var links = context.Create(CreateChapterLinks(chapter: chapter));

        var privacySettings = context.Create(CreateChapterPrivacySettings(chapter: chapter));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.SocialMedia);

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
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        var property = context.Create(CreateChapterProperty(chapter: chapter, name: "prop1"));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Properties);

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
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        var question = context.Create(CreateChapterQuestion(chapter: chapter, name: "q1"));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Questions);

        // Act
        var result = await service.GetChapterQuestionsViewModel(request);

        // Assert
        result.Should().NotBeNull();
        result.Questions.Should().HaveCount(1);
        result.Questions.First().Name.Should().Be("q1");
    }

    [Test]
    public static async Task UpdateChapterAdminMember_WhenValid_ReturnsSuccess()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, otherMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            adminMembers: [currentMember, otherMember]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        var model = CreateChapterAdminMemberUpdateModel();

        // Act
        var result = await service.UpdateChapterAdminMember(request, otherMember.Id, model);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateChapterAdminMember_NoRole_ReturnsFailure()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, otherMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            adminMembers: [currentMember, otherMember]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        var model = CreateChapterAdminMemberUpdateModel(
            role: ChapterAdminRole.None);

        // Act
        var result = await service.UpdateChapterAdminMember(request, otherMember.Id, model);

        // Assert
        result.Success.Should().BeFalse();
    }

    [TestCase(ChapterAdminRole.Owner)]
    public static async Task UpdateChapterAdminMember_OwnerRoleCannotBeSetDirectly(ChapterAdminRole currentAdminRole)
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, otherMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: otherMember,
            afterCreate: x => context.CreateChapterAdminMember(x, currentMember, role: currentAdminRole));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        var model = CreateChapterAdminMemberUpdateModel(
            role: ChapterAdminRole.Admin);

        // Act
        var result = await service.UpdateChapterAdminMember(request, otherMember.Id, model);

        // Assert
        result.Success.Should().BeFalse();
    }

    [TestCase(ChapterAdminRole.Admin, ChapterAdminRole.Owner)]
    public static async Task UpdateChapterAdminMember_LowerRole_CannotUpdateHigherRole(
        ChapterAdminRole currentMemberRole, ChapterAdminRole updateMemberRole)
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (currentMember, otherMember) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            afterCreate: x =>
            {
                context.CreateChapterAdminMember(x, currentMember, currentMemberRole);
                context.CreateChapterAdminMember(x, otherMember, updateMemberRole);
            });

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.AdminMembers);

        var model = CreateChapterAdminMemberUpdateModel(
            adminEmailAddress: "updated@admin.com");

        // Act
        var result = await service.UpdateChapterAdminMember(request, otherMember.Id, model);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public static async Task UpdateChapterImage_WhenInvalidImage_ReturnsFailure()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        var image = context.Create(CreateChapterImage(chapter: chapter));

        var imageService = CreateMockImageService(isValidImage: false);

        var service = CreateChapterAdminService(context, imageService: imageService);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Branding);

        var model = CreateChapterImageUpdateModel();

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
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        var image = context.Create(CreateChapterImage(chapter: chapter));

        var imageService = CreateMockImageService(isValidImage: true);

        var service = CreateChapterAdminService(context, imageService: imageService);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Branding);

        var model = CreateChapterImageUpdateModel();

        // Act
        var result = await service.UpdateChapterImage(request, model);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateChapterDescription_UpdatesSuccessfully()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        var texts = context.Create(CreateChapterTexts(chapter: chapter));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Texts);

        var description = "<p>New Description</p>";

        // Act
        var result = await service.UpdateChapterDescription(request, description);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateChapterTheme_UpdatesSuccessfully()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Branding);
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
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Publish);

        // Act
        var result = await service.PublishChapter(request);

        // Assert
        result.Success.Should().BeTrue();
    }

    private static MockOdkContext CreateMockOdkContext()
    {
        var context = new MockOdkContext();

        context.Add(new SiteEmailSettings { Platform = PlatformType.Default });

        return context;
    }

    private static IUnitOfWork CreateMockUnitOfWork(MockOdkContext? context = null) => MockUnitOfWork.Create(context);

    private static IImageService CreateMockImageService(bool isValidImage, byte[]? processedData = null)
    {
        var mock = new Mock<IImageService>();
        mock.Setup(x => x.IsImage(It.IsAny<byte[]>()))
            .Returns(isValidImage);
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

    private static IGeolocationService CreateMockGeolocationService(
        Country? country)
    {
        var mock = new Mock<IGeolocationService>();
        mock.Setup(x => x.GetTimeZoneFromLocation(It.IsAny<LatLong>()))
            .ReturnsAsync(TimeZoneInfo.FindSystemTimeZoneById("Europe/London"));

        mock.Setup(x => x.GetCountryFromLocation(It.IsAny<LatLong>()))
            .ReturnsAsync(country);
        return mock.Object;
    }

    private static ITopicService CreateMockTopicService()
    {
        var mock = new Mock<ITopicService>();
        mock.Setup(x => x.AddNewChapterTopics(It.IsAny<IMemberChapterServiceRequest>(), It.IsAny<IReadOnlyCollection<NewTopicModel>>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    private static IMemberEmailService CreateMockMemberEmailService()
    {
        var mock = new Mock<IMemberEmailService>();
        mock.Setup(x => x.SendNewGroupEmail(It.IsAny<IMemberChapterServiceRequest>(), It.IsAny<SiteEmailSettings>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    private static ChapterAdminService CreateChapterAdminService(
        MockOdkContext context,
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
        ILoggingService? loggingService = null,
        ChapterAdminServiceSettings? settings = null)
    {
        return new ChapterAdminService(
            CreateMockUnitOfWork(context),
            htmlSanitizer ?? CreateMockHtmlSanitizer(),
            socialMediaService ?? new Mock<ISocialMediaService>().Object,
            notificationService ?? new Mock<INotificationService>().Object,
            imageService ?? CreateMockImageService(isValidImage: true),
            memberEmailService ?? CreateMockMemberEmailService(),
            topicService ?? CreateMockTopicService(),
            settings ?? CreateChapterAdminServiceSettings(),
            siteSubscriptionService ?? new Mock<ISiteSubscriptionService>().Object,
            urlProviderFactory ?? new Mock<IUrlProviderFactory>().Object,
            paymentProviderFactory ?? new Mock<IPaymentProviderFactory>().Object,
            paymentService ?? new Mock<IPaymentService>().Object,
            geolocationService ?? CreateMockGeolocationService(country: context.CreateCountry()),
            loggingService ?? new Mock<ILoggingService>().Object);
    }

    private static ChapterAdminServiceSettings CreateChapterAdminServiceSettings(
        string? defaultCountryCode = null) =>
        new ChapterAdminServiceSettings
        {
            ContactMessageRecaptchaScoreThreshold = 0.5,
            DefaultCountryCode = defaultCountryCode ?? "",
            ReservedSlugs = []
        };

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

    private static IMemberServiceRequest CreateMemberServiceRequest(
        Member currentMember,
        PlatformType? platform = null)
    {
        var mock = new Mock<IMemberServiceRequest>();

        mock.Setup(x => x.CurrentMember)
            .Returns(currentMember);

        mock.Setup(x => x.CurrentMemberOrDefault)
            .Returns(currentMember);

        mock.Setup(x => x.HttpRequestContext)
            .Returns(CreateHttpRequestContext());

        mock.Setup(x => x.Platform)
            .Returns(platform ?? PlatformType.Default);

        return mock.Object;
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
        string? locationName = null,
        LatLong? location = null,
        byte[]? imageData = null,
        IReadOnlyCollection<Guid>? topicIds = null)
        => new ChapterCreateModel
        {
            Name = name ?? "Test Chapter",
            LocationName = locationName ?? "London",
            Location = location ?? new LatLong { Lat = 51.5, Long = -0.1 },
            ImageData = imageData ?? [],
            NewTopics = [],
            TopicIds = topicIds ?? [Guid.NewGuid()]
        };

    private static ChapterProperty CreateChapterProperty(
        Chapter chapter,
        string? displayName = null,
        string? label = null,
        string? name = null,
        bool? required = null,
        DataType? dataType = null)
        => new ChapterProperty
        {
            ChapterId = chapter.Id,
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
        Chapter chapter,
        string? name = null,
        string? answer = null)
        => new ChapterQuestion
        {
            ChapterId = chapter.Id,
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
        bool? receiveNewMemberEmails = null,
        ChapterAdminRole? role = null)
        => new ChapterAdminMemberUpdateModel
        {
            AdminEmailAddress = adminEmailAddress ?? "admin@test.com",
            ReceiveContactEmails = receiveContactEmails ?? true,
            ReceiveEventCommentEmails = receiveEventCommentEmails ?? true,
            ReceiveNewMemberEmails = receiveNewMemberEmails ?? true,
            Role = role ?? ChapterAdminRole.Admin
        };

    private static ChapterImageUpdateModel CreateChapterImageUpdateModel(byte[]? imageData = null)
        => new ChapterImageUpdateModel { ImageData = imageData ?? [1, 2, 3] };

    private static ChapterThemeUpdateModel CreateChapterThemeUpdateModel(
        string? background = null,
        string? color = null)
        => new ChapterThemeUpdateModel
        {
            Background = background ?? "#ffffff",
            Color = color ?? "#000000"
        };

    private static ChapterContactMessage CreateChapterContactMessage(
        Chapter chapter)
        => new ChapterContactMessage
        {
            FromAddress = "",
            Message = "",
            Id = Guid.NewGuid(),
            ChapterId = chapter.Id
        };

    private static ChapterImage CreateChapterImage(Chapter chapter)
        => new ChapterImage { ChapterId = chapter.Id };

    private static ChapterTexts CreateChapterTexts(Chapter chapter)
        => new ChapterTexts
        {
            ChapterId = chapter.Id,
            Description = "Test description",
            WelcomeText = "Welcome to the test chapter",
            RegisterText = "Register here"
        };

    private static ChapterLinks CreateChapterLinks(Chapter chapter)
        => new ChapterLinks
        {
            ChapterId = chapter.Id,
            FacebookName = null,
            InstagramName = null,
            TwitterName = null,
            Version = []
        };

    private static ChapterPrivacySettings CreateChapterPrivacySettings(Chapter chapter)
        => new ChapterPrivacySettings { ChapterId = chapter.Id };
}
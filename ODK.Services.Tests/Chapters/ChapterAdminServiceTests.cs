using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using ODK.Core.Workflows;
using ODK.Services.Chapters.Workflows;
using ODK.Services.Workflows;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.DataTypes;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Pages;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Resources.Resources;
using ODK.Services.Authorization;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Exceptions;
using ODK.Services.Emails;
using ODK.Services.Geolocation;
using ODK.Services.Html;
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
    private const int MigrationWindowDays = 30;

    [Test]
    public static async Task AddChapterAdminMember_WhenMemberNotChapterAdmin_ThrowsException()
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

        // Act
        Func<Task> act = () => service.AddChapterAdminMember(request, member.Id);

        // Assert
        await act.Should().ThrowAsync<OdkNotAuthorizedException>();
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
        result.Message.Should().Be("Member is already a group admin");
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

    [TestCase(0, false)]
    [TestCase(1, true)]
    public static async Task AddChapterAdminMember_WhenOwnerSubscriptionLapsed_GrantsAccessOnlyWithinCooldown(
        int cooldownMonths,
        bool permitted)
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var (owner, otherMember) = (context.CreateMember(), context.CreateMember());

        context.CreateMemberSiteSubscription(
            owner,
            context.CreateSiteSubscription(features: [SiteFeatureType.AdminMembers]),
            expiresUtc: DateTime.UtcNow.AddDays(-1));

        var chapter = context.CreateChapter(owner: owner);

        var service = CreateChapterAdminService(
            context,
            siteSubscriptionCooldown: new SiteSubscriptionCooldown(cooldownMonths));

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: owner,
            securable: ChapterAdminSecurable.AdminMembers);

        // Act
        var result = await service.AddChapterAdminMember(request, otherMember.Id);

        // Assert
        result.Success.Should().Be(permitted);
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
    public static async Task CreateChapter_WhenSubscriptionExpiredWithinCooldown_ReturnsSuccess()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember(
            afterCreate: x => context.CreateMemberSiteSubscription(
                x, expiresUtc: DateTime.UtcNow.AddDays(-1)));

        var service = CreateChapterAdminService(
            context,
            siteSubscriptionCooldown: new SiteSubscriptionCooldown(months: 1));

        var request = CreateMemberServiceRequest(currentMember);
        var model = CreateChapterCreateModel();

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task CreateChapter_WhenNameHasStrayWhitespace_StoresItNormalised()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var service = CreateChapterAdminService(context);

        var request = CreateMemberServiceRequest(currentMember);
        var model = CreateChapterCreateModel(name: "  Test   Chapter  ");

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Test Chapter");
        result.Value.Slug.Should().Be("test-chapter");
    }

    [Test]
    public static async Task CreateChapter_WhenNameTakenDifferingOnlyByWhitespace_ReturnsFailure()
    {
        // Arrange
        // Pins the ordering, not just the normalising: the name has to be normalised *before* the
        // uniqueness check, or "Test  Chapter" is looked up verbatim, found to be free, and created
        // alongside "Test Chapter" - two names competing for one slug.
        using var context = CreateMockOdkContext();

        context.CreateChapter(name: "Test Chapter");

        var currentMember = context.CreateMember(
            createSiteSubscription: true);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberServiceRequest(currentMember);
        var model = CreateChapterCreateModel(name: "  Test  Chapter ");

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(ErrorMessagesResource.NameTaken.Replace("{name}", "Test Chapter"));
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
    public static async Task CreateChapter_WhenSlugReserved_AppendsVersion()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var service = CreateChapterAdminService(
            context, settings: CreateChapterAdminServiceSettings(reservedSlugs: ["new"]));

        var request = CreateMemberServiceRequest(currentMember);
        var model = CreateChapterCreateModel(name: "New");

        // Act
        var result = await service.CreateChapter(request, model);

        // Assert
        result.Value.Should().NotBeNull();
        result.Value.Slug.Should().Be("new-2");
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
    public static async Task CreateChapterProperty_WhenMissingDisplayName_ReturnsFailure()
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

        var model = CreateChapterPropertyCreateModel(displayName: string.Empty);

        // Act
        var result = await service.CreateChapterProperty(request, model);

        // Assert
        result.Success.Should().BeFalse();
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
    public static async Task GetChapterSettingsViewModel_ReturnsSocialMediaLinks()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            siteSubscription: context.CreateSiteSubscription(features: [SiteFeatureType.AdminMembers]),
            adminMembers: [currentMember]);

        var links = context.Create(CreateChapterLinks(chapter: chapter));

        context.Create(CreateChapterPrivacySettings(chapter: chapter));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.GroupSettings);

        // Act
        var result = await service.GetChapterSettingsViewModel(request);

        // Assert
        result.Links.Should().NotBeNull();
        result.Links.Links.Should().Be(links);
    }

    [Test]
    public static async Task GetChapterSettingsViewModel_WithThemeFeature_ThemeCanBeEdited()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(
            adminMembers: [currentMember],
            siteSubscription: context.CreateSiteSubscription(features: [SiteFeatureType.Theme]));

        var service = CreateChapterAdminService(context);
        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.GroupSettings);

        // Act
        var result = await service.GetChapterSettingsViewModel(request);

        // Assert
        result.Theme.Should().NotBeNull();
        result.Theme.CanEdit.Should().BeTrue();
    }

    [Test]
    public static async Task GetChapterSettingsViewModel_WithoutThemeFeature_ThemeCannotBeEdited()
    {
        // Arrange - a different feature, so this proves the check is for Theme specifically rather than
        // for holding any subscription at all.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(
            adminMembers: [currentMember],
            siteSubscription: context.CreateSiteSubscription(features: [SiteFeatureType.AdminMembers]));

        var service = CreateChapterAdminService(context);
        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.GroupSettings);

        // Act
        var result = await service.GetChapterSettingsViewModel(request);

        // Assert
        result.Theme.Should().NotBeNull();
        result.Theme.CanEdit.Should().BeFalse();
    }

    [Test]
    public static async Task GetChapterSettingsViewModel_ChapterOnAnotherPlatform_OffersThatPlatformsPages()
    {
        /* Arrange - a Drunken Knitwits group, administered from Group Squirrel. Its set of pages is its own,
           so the About page Drunken Knitwits groups have stays editable from either site. Group Squirrel's
           own groups have no About page, which is what makes the two sets distinguishable here. */
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(
            owner: currentMember,
            platform: PlatformType.DrunkenKnitwits);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            platform: PlatformType.GroupSquirrel,
            securable: ChapterAdminSecurable.GroupSettings);

        // Act
        var result = await service.GetChapterSettingsViewModel(request);

        // Assert
        result.Pages.Should().NotBeNull();
        result.Pages.ChapterPages.Select(x => x.PageType).Should()
            .BeEquivalentTo([PageType.About, PageType.Contact, PageType.Members]);
    }

    [Test]
    public static async Task GetChapterSettingsViewModel_DrunkenKnitwits_OmitsSectionsTheGroupDoesNotOwn()
    {
        /* Arrange - administered from Drunken Knitwits, where branding, the picture, topics, location and
           pages are not a group's own to set. The sections that are stay, so this distinguishes a platform
           gate from the page coming back empty. */
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(
            adminMembers: [currentMember],
            platform: PlatformType.DrunkenKnitwits);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            platform: PlatformType.DrunkenKnitwits,
            securable: ChapterAdminSecurable.GroupSettings);

        // Act
        var result = await service.GetChapterSettingsViewModel(request);

        // Assert
        result.Theme.Should().BeNull();
        result.Image.Should().BeNull();
        result.Location.Should().BeNull();
        result.Pages.Should().BeNull();
        result.Topics.Should().BeNull();
        result.Links.Should().NotBeNull();
        result.Privacy.Should().NotBeNull();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenPublishedGroupHasNoMovedPage_PromptsMovedPage()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember,
            afterCreate: x => x.PublishedUtc = DateTime.UtcNow);

        context.CreateChapterImage(chapter);

        context.CreateChapterTexts(chapter);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert - an offer rather than an outstanding action, so it doesn't make the group look busy.
        result.PromptMovedPage.Should().BeTrue();
        result.HasRequiredActions.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenGroupNotPublished_DoesNotPromptMovedPage()
    {
        // Arrange - a moved page points at a group nobody outside it can see yet, so publishing comes first.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(approvedUtc: DateTime.UtcNow, owner: currentMember);

        context.CreateChapterImage(chapter);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.PromptMovedPage.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenMovedPagePublished_DoesNotPromptMovedPage()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = CreatePublishedChapter(context, currentMember);

        context.Create(new ChapterMigration
        {
            ChapterId = chapter.Id,
            MovedUtc = DateTime.UtcNow
        });

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.PromptMovedPage.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenMovedPagePublished_PromptsToShareIt()
    {
        /* Arrange - the moved page existing is half the job; the outstanding action is telling people it
           does, and nobody goes looking for wording they do not know is there. */
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = CreatePublishedChapter(context, currentMember);

        context.Create(new ChapterMigration
        {
            ChapterId = chapter.Id,
            MovedUtc = DateTime.UtcNow
        });

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.PromptShareMovedPage.Should().BeTrue();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenTheMoveIsOld_StopsPromptingToShareIt()
    {
        /* Arrange - after the migration window the move is not news, and a panel nobody can clear is a
           panel everybody stops reading. */
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = CreatePublishedChapter(context, currentMember);

        context.Create(new ChapterMigration
        {
            ChapterId = chapter.Id,
            MovedUtc = DateTime.UtcNow.AddDays(-(MigrationWindowDays + 1))
        });

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.PromptShareMovedPage.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenNoMovedPage_DoesNotPromptToShareIt()
    {
        // Arrange - there is nothing to tell anyone about yet.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = CreatePublishedChapter(context, currentMember);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.PromptShareMovedPage.Should().BeFalse();
    }

    [Test]
    public static async Task GetChapterMigrationViewModel_BuildsWordingNamingTheGroupAndItsAddress()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = CreatePublishedChapter(context, currentMember, name: "Bristol Knitters");

        context.Create(new ChapterMigration
        {
            ChapterId = chapter.Id,
            MovedUtc = DateTime.UtcNow,
            PreviousPlatformName = "Meetup"
        });

        var service = CreateChapterAdminService(
            context, urlProviderFactory: CreateMockUrlProviderFactory(chapter));

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MovedPage);

        // Act
        var result = await service.GetChapterMigrationViewModel(request);

        // Assert
        result.Share.ShortMessage.Should().Contain("Bristol Knitters");
        result.Share.ShortMessage.Should().Contain("from Meetup");
        result.Share.ShortMessage.Should().Contain(result.MovedPageUrl);
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenPromptDismissed_DoesNotPromptMovedPage()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = CreatePublishedChapter(context, currentMember);

        context.Create(new ChapterMigration
        {
            ChapterId = chapter.Id,
            PromptDismissedUtc = DateTime.UtcNow
        });

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.PromptMovedPage.Should().BeFalse();
    }

    [Test]
    public static async Task DismissMovedPagePrompt_WhenGroupHasNoMigration_CreatesOneDismissed()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = CreatePublishedChapter(context, currentMember);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.DismissMovedPagePrompt(request);

        // Assert
        result.Success.Should().BeTrue();

        var migration = context.Set<ChapterMigration>().Single(x => x.ChapterId == chapter.Id);
        migration.PromptDismissedUtc.Should().NotBeNull();
        migration.MovedUtc.Should().BeNull();
    }

    [Test]
    public static async Task DismissMovedPagePrompt_WhenAlreadyDismissed_KeepsTheOriginalDate()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = CreatePublishedChapter(context, currentMember);

        var dismissedUtc = DateTime.UtcNow.AddDays(-10);

        context.Create(new ChapterMigration
        {
            ChapterId = chapter.Id,
            MessageHtml = "<p>Somewhere better</p>",
            PromptDismissedUtc = dismissedUtc
        });

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        await service.DismissMovedPagePrompt(request);

        // Assert - and the wording the organiser wrote is left alone.
        var migration = context.Set<ChapterMigration>().Single(x => x.ChapterId == chapter.Id);
        migration.PromptDismissedUtc.Should().Be(dismissedUtc);
        migration.MessageHtml.Should().Be("<p>Somewhere better</p>");
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenGroupHasNoPicture_LeavesTheStepOutstanding()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert - the picture is a checklist step, and nothing is waiting on the admin outside it.
        result.Checklist.Should().NotBeNull();
        ChecklistStep(result, ChecklistItemType.Picture).IsResolved().Should().BeFalse();
        result.HasRequiredActions.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenApprovedGroupHasNoPicture_CannotPublishYet()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember,
            afterCreate: x => x.SubmittedForApprovalUtc = DateTime.UtcNow);

        context.CreateChapterTexts(chapter);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert - approval is behind it, publication is not, and the picture is why.
        result.Checklist.Should().NotBeNull();
        result.Checklist!.CanPublish.Should().BeFalse();
        ChecklistStep(result, ChecklistItemType.SubmitForApproval).IsCompleted().Should().BeTrue();
        ChecklistStep(result, ChecklistItemType.Publish).IsResolved().Should().BeFalse();
        ChecklistStep(result, ChecklistItemType.Picture).IsResolved().Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenPublishedGroupHasNoPicture_StillRequiresOne()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember,
            afterCreate: x => x.PublishedUtc = DateTime.UtcNow);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert - publishing is behind it, so the checklist outlives publication for the rest.
        result.Checklist.Should().NotBeNull();
        ChecklistStep(result, ChecklistItemType.Publish).IsCompleted().Should().BeTrue();
        ChecklistStep(result, ChecklistItemType.Picture).IsResolved().Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenApprovedGroupHasAPicture_CanPublish()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember);

        context.CreateChapterImage(chapter);

        context.CreateChapterTexts(chapter);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.Checklist.Should().NotBeNull();
        result.Checklist!.CanPublish.Should().BeTrue();
        ChecklistStep(result, ChecklistItemType.Picture).IsCompleted().Should().BeTrue();

        // Publishing is the checklist's business, not an action needing attention.
        result.HasRequiredActions.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenAStepCompletes_RecordsItOnce()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        context.CreateChapterImage(chapter);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act - loaded twice, so a second load has the chance to write the row again.
        await service.GetGroupDashboardViewModel(request);
        var recordedAfterFirst = context.Set<ChapterChecklistItem>()
            .Where(x => x.ChapterId == chapter.Id && x.ChecklistItemType == ChecklistItemType.Picture)
            .Select(x => x.CompletedUtc)
            .Single();

        await service.GetGroupDashboardViewModel(request);

        // Assert - one row, and the timestamp is the first sighting rather than the latest.
        var recorded = context.Set<ChapterChecklistItem>()
            .Where(x => x.ChapterId == chapter.Id && x.ChecklistItemType == ChecklistItemType.Picture)
            .ToArray();
        recorded.Length.Should().Be(1);
        recorded[0].CompletedUtc.Should().Be(recordedAfterFirst);
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenAStepIsDated_UsesItsOwnTimestamp()
    {
        // Arrange - the group was created and approved long before anyone looked at a checklist.
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var submittedUtc = DateTime.UtcNow.AddDays(-30);

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow.AddDays(-20),
            owner: currentMember,
            afterCreate: x => x.SubmittedForApprovalUtc = submittedUtc);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert - the step carries the date it happened, not the date it was noticed.
        ChecklistStep(result, ChecklistItemType.SubmitForApproval).CompletedUtc.Should().Be(submittedUtc);
        ChecklistStep(result, ChecklistItemType.CreateGroup).CompletedUtc.Should().Be(chapter.CreatedUtc);
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenEveryStepIsResolved_DropsTheChecklist()
    {
        // Arrange - every step already recorded, which is what a finished setup looks like.
        using var context = CreateMockOdkContext();

        var items = context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        foreach (var item in items)
        {
            context.Create(new ChapterChecklistItem
            {
                ChapterId = chapter.Id,
                ChecklistItemType = item.Type,
                CompletedUtc = DateTime.UtcNow
            });
        }

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.Checklist.Should().BeNull();
    }

    [Test]
    public static async Task DismissChecklistItem_WhenStepIsDismissable_TakesItOffTheChecklist()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.DismissChecklistItem(request, ChecklistItemType.Topics);

        // Assert
        result.Success.Should().BeTrue();

        var recorded = context.Set<ChapterChecklistItem>()
            .Single(x => x.ChapterId == chapter.Id && x.ChecklistItemType == ChecklistItemType.Topics);
        recorded.DismissedUtc.Should().NotBeNull();
        recorded.CompletedUtc.Should().BeNull();
    }

    [Test]
    public static async Task DismissChecklistItem_WhenStepIsRequired_Fails()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.DismissChecklistItem(request, ChecklistItemType.Picture);

        // Assert - and nothing is recorded, so the step stays exactly as it was.
        result.Success.Should().BeFalse();
        context.Set<ChapterChecklistItem>()
            .Any(x => x.ChapterId == chapter.Id)
            .Should().BeFalse();
    }

    [Test]
    public static async Task GetMembershipSettingsViewModel_WhenFirstOpened_RecordsTheStep()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            owner: currentMember,
            siteSubscription: context.CreateSiteSubscription(
                features: [SiteFeatureType.MemberSubscriptions]));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        await service.GetMembershipSettingsViewModel(request);

        // Assert - the settings have defaults that are right for most groups, so looking is the evidence.
        context.Set<ChapterChecklistItem>()
            .Single(x => x.ChapterId == chapter.Id
                && x.ChecklistItemType == ChecklistItemType.MembershipSettings)
            .CompletedUtc.Should().NotBeNull();
    }

    [Test]
    public static async Task GetMembershipSettingsViewModel_WhenTheOwnerCannotUseThem_RecordsNothing()
    {
        // Arrange - without the feature the page shows what the plan would buy, so there is nothing on it
        // to have reviewed.
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        await service.GetMembershipSettingsViewModel(request);

        // Assert
        context.Set<ChapterChecklistItem>()
            .Any(x => x.ChapterId == chapter.Id)
            .Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenTheOwnerCannotUseMembershipSettings_DropsTheStep()
    {
        // Arrange - a step behind a feature the owner does not pay for is absent rather than outstanding.
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert - and it is not merely hidden: a step left in and never completable would hold the
        // checklist open for good, so it is out of the count as well.
        result.Checklist.Should().NotBeNull();
        result.Checklist!.Items.Should().NotContain(x => x.Type == ChecklistItemType.MembershipSettings);
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenTheOwnerCanUseMembershipSettings_KeepsTheStep()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            owner: currentMember,
            siteSubscription: context.CreateSiteSubscription(
                features: [SiteFeatureType.MemberSubscriptions]));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.Checklist.Should().NotBeNull();
        result.Checklist!.Items.Should().Contain(x => x.Type == ChecklistItemType.MembershipSettings);
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenPublishedGroupHoldsInvites_OffersToSendThem()
    {
        // Arrange - an import raised the invites while nobody outside the group could see it.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember,
            afterCreate: x => x.PublishedUtc = DateTime.UtcNow);

        context.CreateChapterImage(chapter);

        CreateInvite(context, chapter.Id, context.CreateMember().Id);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.HeldInvites.Should().Be(1);
        result.CanSendHeldInvites.Should().BeTrue();
        result.HasRequiredActions.Should().BeTrue();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenUnpublishedGroupHoldsInvites_DoesNotOfferToSendThem()
    {
        /* Arrange - the invites are counted, because publishing says what it makes sendable, but sending
           them is not yet an action: an invite's link lands on a group nobody outside it can see. */
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember);

        context.CreateChapterImage(chapter);

        context.CreateChapterTexts(chapter);

        CreateInvite(context, chapter.Id, context.CreateMember().Id);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.HeldInvites.Should().Be(1);
        result.CanSendHeldInvites.Should().BeFalse();
        result.HasRequiredActions.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenHeldInvitesHaveBeenSent_DoesNotOfferToSendThem()
    {
        // Arrange - a sent invite is not being held, so there is nothing left to act on.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember,
            afterCreate: x => x.PublishedUtc = DateTime.UtcNow);

        context.CreateChapterImage(chapter);

        context.Create(new MemberChapterInvite
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            Id = Guid.NewGuid(),
            MemberId = context.CreateMember().Id,
            SentUtc = DateTime.UtcNow.AddDays(-1),
            Token = Guid.NewGuid().ToString()
        });

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.HeldInvites.Should().Be(0);
        result.CanSendHeldInvites.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenNothingIsOutstanding_HasNoRequiredActions()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember,
            afterCreate: x => x.PublishedUtc = DateTime.UtcNow);

        context.CreateChapterImage(chapter);
        context.CreateChapterTexts(chapter);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.HasRequiredActions.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenGroupHasNoShortDescription_LeavesTheStepOutstanding()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        ChecklistStep(result, ChecklistItemType.Description).IsResolved().Should().BeFalse();
        result.HasRequiredActions.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenGroupShortDescriptionIsBlank_LeavesTheStepOutstanding()
    {
        // Arrange - a texts row exists but says nothing, which is the same gap as having no row at all.
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        context.CreateChapterTexts(chapter, shortDescription: " ");

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        ChecklistStep(result, ChecklistItemType.Description).IsResolved().Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenGroupHasOnlyAShortDescription_LeavesTheStepOutstanding()
    {
        // Arrange - the form requires both, so a group with one of them has not finished describing itself.
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        context.CreateChapterTexts(chapter);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        ChecklistStep(result, ChecklistItemType.Description).IsResolved().Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenGroupHasBothDescriptions_CompletesTheStep()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        context.CreateChapterTexts(chapter, descriptionHtml: "<p>What we do</p>");

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert - one step off two fields, because the form asks for both before it will save.
        ChecklistStep(result, ChecklistItemType.Description).IsCompleted().Should().BeTrue();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenNobodyElseHasJoined_PromptsMemberImport()
    {
        // Arrange - a group with nothing else outstanding, so the prompt stands on its own.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember,
            afterCreate: x => x.PublishedUtc = DateTime.UtcNow);

        context.CreateChapterImage(chapter);

        context.CreateChapterTexts(chapter);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert - an invitation rather than an outstanding action, so it doesn't make the group look busy.
        result.PromptMemberImport.Should().BeTrue();
        result.HasRequiredActions.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenAnotherMemberHasJoined_DoesNotPromptMemberImport()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            owner: currentMember,
            members: [context.CreateMember()]);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.PromptMemberImport.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenAddressesHaveBeenUploaded_DoesNotPromptMemberImport()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        context.Create(new MemberChapterImport
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            EmailAddress = "imported@example.com",
            Id = Guid.NewGuid(),
            UploadedUtc = DateTime.UtcNow
        });

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        /* Assert - the import has already happened, so the prompt to start one has nothing to say. The
           rows are counted; whether inviting them is outstanding is a separate question of publication. */
        result.PromptMemberImport.Should().BeFalse();
        result.WaitingToBeInvited.Should().Be(1);
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenPublishedGroupHasUploadedAddresses_OffersToInviteThem()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember,
            afterCreate: x => x.PublishedUtc = DateTime.UtcNow);

        context.CreateChapterImage(chapter);

        CreateUpload(context, chapter.Id);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.CanInviteUploaded.Should().BeTrue();
        result.HasRequiredActions.Should().BeTrue();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenUnpublishedGroupHasUploadedAddresses_DoesNotOfferToInviteThem()
    {
        /* Arrange - a group can build its list before anyone can see it, but cannot act on it: an invite's
           link lands on the group. The rows wait on publication rather than on an admin. */
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember);

        context.CreateChapterImage(chapter);

        context.CreateChapterTexts(chapter);

        CreateUpload(context, chapter.Id);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.WaitingToBeInvited.Should().Be(1);
        result.CanInviteUploaded.Should().BeFalse();
        result.HasRequiredActions.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenInvitesHaveBeenSent_DoesNotPromptMemberImport()
    {
        // Arrange - inviting an address deletes the import row it came from, so a group that has finished
        // an import holds invites and nothing else until somebody accepts one.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        context.Create(new MemberChapterInvite
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            MemberId = context.CreateMember().Id,
            SentUtc = DateTime.UtcNow
        });

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.PromptMemberImport.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_ReturnsUpcomingEventsSoonestFirst()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        var chapterVenue = context.CreateChapterVenue(chapter);

        context.CreateEvent(chapter, chapterVenue, date: DateTime.UtcNow.AddDays(-1));
        context.CreateEvent(chapter, chapterVenue, date: DateTime.UtcNow.AddDays(4));
        context.CreateEvent(chapter, chapterVenue, date: DateTime.UtcNow.AddDays(1));
        context.CreateEvent(chapter, chapterVenue, date: DateTime.UtcNow.AddDays(3));
        context.CreateEvent(chapter, chapterVenue, date: DateTime.UtcNow.AddDays(2));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert - the past event is left out, and only as many as the dashboard shows are loaded.
        result.UpcomingEvents.Should().NotBeNull();
        result.UpcomingEvents!.Should().HaveCount(3);
        result.UpcomingEvents!
            .Select(x => x.Event.DateUtc)
            .Should()
            .BeInAscendingOrder();
        result.UpcomingEvents!.First().Event.DateUtc
            .Should().BeCloseTo(DateTime.UtcNow.AddDays(1), TimeSpan.FromMinutes(1));
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_ReturnsMostRecentlyJoinedMembersFirst()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var members = Enumerable.Range(0, 5)
            .Select(_ => context.CreateMember())
            .ToArray();

        var chapter = context.CreateChapter(
            owner: currentMember,
            members: members);

        // Joining is what orders this, not signing up, so the join dates are set apart deliberately.
        for (var i = 0; i < members.Length; i++)
        {
            members[i].Chapters.Single(x => x.ChapterId == chapter.Id).CreatedUtc =
                DateTime.UtcNow.AddDays(-i);
        }

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert - the oldest join is dropped, the rest are newest first.
        result.NewestMembers.Should().NotBeNull();
        result.NewestMembers!
            .Select(x => x.Member.Id)
            .Should()
            .Equal(members.Take(4).Select(x => x.Id));
    }

    // Has to agree with CreateChapter, which normalises before its own uniqueness check - if this one
    // did not, it would report a name as free and the submit that follows would reject it as taken.
    [TestCase("Test Chapter Two", ExpectedResult = true)]
    [TestCase("Test Chapter", ExpectedResult = false)]
    [TestCase("  Test  Chapter ", ExpectedResult = false)]
    public static async Task<bool> NameIsAvailable_NormalisesNameBeforeChecking(string name)
    {
        // Arrange
        using var context = CreateMockOdkContext();

        context.CreateChapter(name: "Test Chapter");

        var currentMember = context.CreateMember();

        var service = CreateChapterAdminService(context);

        var request = CreateMemberServiceRequest(currentMember);

        // Act
        var result = await service.NameIsAvailable(request, name);

        // Assert
        return result;
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
            adminMembers: [currentMember],
            siteSubscription: context.CreateSiteSubscription(features: [SiteFeatureType.Theme]));

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
    public static async Task UpdateChapterTexts_UpdatesSuccessfully()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        context.Create(CreateChapterTexts(chapter: chapter));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Texts);

        var model = CreateChapterTextsUpdateModel();

        // Act
        var result = await service.UpdateChapterTexts(request, model);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateChapterTexts_RequiredFieldIsMissing_NamesTheField()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        context.Create(CreateChapterTexts(chapter: chapter));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Texts);

        var model = CreateChapterTextsUpdateModel(registerTextHtml: "");

        // Act
        var result = await service.UpdateChapterTexts(request, model);

        // Assert
        result.Messages.Should().Equal($"{ChapterTextLabels.RegisterText} is required");
    }

    [Test]
    public static async Task UpdateChapterTexts_SeveralFieldsAreInvalid_ReportsEachOne()
    {
        // Arrange - one field empty and another holding markup the validator rejects, so a single save has
        // to account for both rather than stopping at the first.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        context.Create(CreateChapterTexts(chapter: chapter));

        var service = CreateChapterAdminService(
            context,
            htmlValidator: CreateMockHtmlValidator(rejected: "<script>"));

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Texts);

        var model = CreateChapterTextsUpdateModel(
            descriptionHtml: "<script>alert(1)</script>",
            welcomeTextHtml: "");

        // Act
        var result = await service.UpdateChapterTexts(request, model);

        // Assert - in the order the fields appear on the form.
        result.Messages.Should().Equal(
            $"{ChapterTextLabels.Description}: Unsupported HTML: <script>",
            $"{ChapterTextLabels.WelcomeText} is required");
    }

    [Test]
    public static async Task UpdateChapterTexts_StoredTextIsInvalidAndUnchanged_UpdatesSuccessfully()
    {
        // Arrange - markup stored before the validator existed must not block an edit to another field on
        // the same form.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember]);

        context.Create(CreateChapterTexts(chapter: chapter, descriptionHtml: "<script>alert(1)</script>"));

        var service = CreateChapterAdminService(
            context,
            htmlValidator: CreateMockHtmlValidator(rejected: "<script>"));

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Texts);

        var model = CreateChapterTextsUpdateModel(descriptionHtml: "<script>alert(1)</script>");

        // Act
        var result = await service.UpdateChapterTexts(request, model);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateChapterTexts_PlatformWithoutShortDescription_LeavesTheStoredOneAlone()
    {
        // Arrange - Drunken Knitwits does not offer the box, so its post carries nothing for it. A group
        // that moved platforms keeps whatever summary it already had.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember],
            platform: PlatformType.DrunkenKnitwits);

        var texts = context.Create(
            CreateChapterTexts(chapter: chapter, shortDescription: "A group worth joining"));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            platform: PlatformType.DrunkenKnitwits,
            securable: ChapterAdminSecurable.Texts);

        var model = CreateChapterTextsUpdateModel(shortDescription: string.Empty);

        // Act
        var result = await service.UpdateChapterTexts(request, model);

        // Assert
        result.Success.Should().BeTrue();

        texts.ShortDescription.Should().Be("A group worth joining");
    }

    [Test]
    public static async Task UpdateChapterTexts_PlatformWithShortDescriptionPostsAnEmptyOne_NamesTheField()
    {
        // Arrange - where the box is offered it is required, so emptying it is not a way to clear it.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(adminMembers: [currentMember]);

        var texts = context.Create(
            CreateChapterTexts(chapter: chapter, shortDescription: "A group worth joining"));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            platform: PlatformType.GroupSquirrel,
            securable: ChapterAdminSecurable.Texts);

        var model = CreateChapterTextsUpdateModel(shortDescription: string.Empty);

        // Act
        var result = await service.UpdateChapterTexts(request, model);

        // Assert - and the stored one is left as it was, since nothing was written.
        result.Messages.Should().Equal($"{ChapterTextLabels.ShortDescription} is required");

        texts.ShortDescription.Should().Be("A group worth joining");
    }

    [Test]
    public static async Task UpdateChapterTexts_PlatformWithoutShortDescriptionPostsAnEmptyOne_Succeeds()
    {
        /* Arrange - the ghost field. Drunken Knitwits never renders the box, so every one of its posts
           carries nothing for it; a requirement that did not ask which platform was posting would fail
           every save on that platform against a field nobody was shown. */
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember],
            platform: PlatformType.DrunkenKnitwits);

        context.Create(CreateChapterTexts(chapter: chapter, shortDescription: null));

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            platform: PlatformType.DrunkenKnitwits,
            securable: ChapterAdminSecurable.Texts);

        var model = CreateChapterTextsUpdateModel(shortDescription: string.Empty);

        // Act
        var result = await service.UpdateChapterTexts(request, model);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateChapterMembershipSettings_OwnerHasApproveMembers_AppliesTheSetting()
    {
        // Arrange - the owner pays for member approval, so turning it on has to stick. This condition was
        // once inverted: it applied the setting only to owners *without* the feature, so a paying owner's
        // choice was discarded while a free owner's was stored and then ignored at join time.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(
            adminMembers: [currentMember],
            siteSubscription: context.CreateSiteSubscription(
                features: [SiteFeatureType.MemberSubscriptions, SiteFeatureType.ApproveMembers]));

        var service = CreateChapterAdminService(context);
        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MembershipSettings);

        // Act
        var result = await service.UpdateChapterMembershipSettings(
            request, CreateMembershipSettingsUpdateModel(approveNewMembers: true));

        // Assert
        result.Success.Should().BeTrue();
        var settings = context.Set<ChapterMembershipSettings>().Single(x => x.ChapterId == chapter.Id);
        settings.ApproveNewMembers.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateChapterMembershipSettings_OwnerLacksApproveMembers_IgnoresTheSetting()
    {
        // Arrange - without the feature the setting must not be stored, so it can't quietly take effect if
        // the subscription later changes. MemberSubscriptions is still needed to pass the outer guard.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(
            adminMembers: [currentMember],
            siteSubscription: context.CreateSiteSubscription(
                features: [SiteFeatureType.MemberSubscriptions]));

        var service = CreateChapterAdminService(context);
        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.MembershipSettings);

        // Act
        var result = await service.UpdateChapterMembershipSettings(
            request, CreateMembershipSettingsUpdateModel(approveNewMembers: true));

        // Assert
        result.Success.Should().BeTrue();
        var settings = context.Set<ChapterMembershipSettings>().Single(x => x.ChapterId == chapter.Id);
        settings.ApproveNewMembers.Should().BeFalse();
    }

    [Test]
    public static async Task UpdateChapterTheme_WithoutThemeFeature_ReturnsFailure()
    {
        // Arrange - the admin page renders read-only without the feature, but that is presentation only.
        // This is what actually withholds the change, so a hand-crafted post can't get round it.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(adminMembers: [currentMember]);

        var service = CreateChapterAdminService(context);
        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Branding);

        // Act
        var result = await service.UpdateChapterTheme(request, CreateChapterThemeUpdateModel());

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Not permitted");
    }

    [Test]
    public static async Task UpdateChapterTheme_WithoutThemeFeature_LeavesTheExistingThemeAlone()
    {
        // Arrange - losing the feature must not strip a theme the group already has; it keeps rendering,
        // only editing is withheld.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(
            adminMembers: [currentMember],
            afterCreate: x =>
            {
                x.ThemeBackground = "#111111";
                x.ThemeColor = "#222222";
            });

        var service = CreateChapterAdminService(context);
        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Branding);

        // Act
        await service.UpdateChapterTheme(request, CreateChapterThemeUpdateModel());

        // Assert
        chapter.ThemeBackground.Should().Be("#111111");
        chapter.ThemeColor.Should().Be("#222222");
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

        context.CreateChapterImage(chapter);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Publish);

        // Act
        var result = await service.PublishChapter(request);

        // Assert
        result.Success.Should().BeTrue();
        chapter.IsPublished().Should().BeTrue();
    }

    [Test]
    public static async Task PublishChapter_WhenHoldingInvites_TellsTheOwnerTheyAreWaiting()
    {
        /* Arrange - an unpublished group can prepare a member import, and publishing gives those invites
           somewhere to land. Sending them is the owner's own action, so publishing only says so. */
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember);

        context.CreateChapterImage(chapter);

        var invited = context.CreateMember();
        CreateInvite(context, chapter.Id, invited.Id);

        var memberEmailService = new Mock<IMemberEmailService>();
        var service = CreateChapterAdminService(context, memberEmailService: memberEmailService.Object);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Publish);

        // Act
        var result = await service.PublishChapter(request);

        // Assert
        result.Success.Should().BeTrue();

        memberEmailService.Verify(
            x => x.SendInvitesWaitingEmail(
                It.Is<IChapterServiceRequest>(y => y.Chapter.Id == chapter.Id),
                It.Is<Member>(y => y.Id == currentMember.Id),
                1),
            Times.Once);
    }

    [Test]
    public static async Task PublishChapter_WhenHoldingInvites_DoesNotSendThem()
    {
        /* Arrange - the invites wait for the owner to send them, so publishing emails nobody who was
           imported. Recording one as sent here would leave it holding an invite nobody ever received. */
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember);

        context.CreateChapterImage(chapter);

        var invited = context.CreateMember();
        var invite = CreateInvite(context, chapter.Id, invited.Id);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Publish);

        // Act
        var result = await service.PublishChapter(request);

        // Assert
        result.Success.Should().BeTrue();

        context.Set<MemberChapterInvite>()
            .Single(x => x.Id == invite.Id)
            .SentUtc
            .Should()
            .BeNull();
    }

    [Test]
    public static async Task PublishChapter_WhenHoldingNoInvites_TellsTheOwnerNothing()
    {
        // Arrange - which is every group that imported nobody before publishing.
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            owner: currentMember);

        context.CreateChapterImage(chapter);

        var memberEmailService = new Mock<IMemberEmailService>();
        var service = CreateChapterAdminService(context, memberEmailService: memberEmailService.Object);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Publish);

        // Act
        var result = await service.PublishChapter(request);

        // Assert
        result.Success.Should().BeTrue();

        memberEmailService.Verify(
            x => x.SendInvitesWaitingEmail(
                It.IsAny<IChapterServiceRequest>(), It.IsAny<Member>(), It.IsAny<int>()),
            Times.Never);
    }

    [Test]
    public static async Task PublishChapter_WhenNoImage_ReturnsFailure()
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
        result.Success.Should().BeFalse();
        result.Message.Should().Be("This group needs a picture before it can be published");
        chapter.IsPublished().Should().BeFalse();
    }

    [Test]
    public static async Task PublishChapter_WhenNotApproved_ReturnsFailure()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        context.CreateChapterImage(chapter);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.Publish);

        // Act
        var result = await service.PublishChapter(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("This group cannot be published");
        chapter.IsPublished().Should().BeFalse();
    }

    [Test]
    public static async Task SubmitChapterForApproval_StepsAboveAreOutstanding_RefusesWithoutRecordingAnything()
    {
        // Arrange - a group that has done nothing beyond existing.
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.SubmitForApproval);

        // Act
        var result = await service.SubmitChapterForApproval(request);

        // Assert
        result.Success.Should().BeFalse();
        chapter.SubmittedForApprovalUtc.Should().BeNull();
    }

    [Test]
    public static async Task SubmitChapterForApproval_StepsAboveAreDone_RecordsItAndTellsSiteAdmins()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();
        context.CreateMember(siteAdmin: true);

        var chapter = ChapterReadyToSubmit(context, currentMember);

        var emailService = new Mock<IMemberEmailService>();

        var service = CreateChapterAdminService(context, memberEmailService: emailService.Object);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.SubmitForApproval);

        // Act
        var result = await service.SubmitChapterForApproval(request);

        // Assert
        result.Success.Should().BeTrue();
        chapter.SubmittedForApprovalUtc.Should().NotBeNull();

        emailService.Verify(
            x => x.SendGroupSubmittedEmail(
                It.IsAny<IServiceRequest>(),
                It.IsAny<Chapter>(),
                It.IsAny<IEnumerable<Member>>()),
            Times.Once);
    }

    [Test]
    public static async Task SubmitChapterForApproval_AlreadySubmitted_SucceedsWithoutTellingAnyoneAgain()
    {
        // Arrange - submitting twice is not a mistake, so it reports success and does nothing.
        using var context = CreateMockOdkContext();

        var submittedUtc = DateTime.UtcNow.AddDays(-3);

        var currentMember = context.CreateMember();

        var chapter = ChapterReadyToSubmit(context, currentMember);
        chapter.SubmittedForApprovalUtc = submittedUtc;

        var emailService = new Mock<IMemberEmailService>();

        var service = CreateChapterAdminService(context, memberEmailService: emailService.Object);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember,
            securable: ChapterAdminSecurable.SubmitForApproval);

        // Act
        var result = await service.SubmitChapterForApproval(request);

        // Assert - and the date it first asked is left alone.
        result.Success.Should().BeTrue();
        chapter.SubmittedForApprovalUtc.Should().Be(submittedUtc);

        emailService.Verify(
            x => x.SendGroupSubmittedEmail(
                It.IsAny<IServiceRequest>(),
                It.IsAny<Chapter>(),
                It.IsAny<IEnumerable<Member>>()),
            Times.Never);
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_StepsAboveSubmissionAreDone_OffersToSubmit()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = ChapterReadyToSubmit(context, currentMember);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.Checklist.Should().NotBeNull();
        result.Checklist!.CanSubmitForApproval.Should().BeTrue();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_StepsAboveSubmissionAreOutstanding_DoesNotOfferToSubmit()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        context.CreateChecklistItems();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.Checklist.Should().NotBeNull();
        result.Checklist!.CanSubmitForApproval.Should().BeFalse();
    }

    /// <summary>
    /// A group with every step above submission behind it: the ones a fact settles, and the two that only
    /// a page view can. Membership settings are absent from its checklist, the owner having no
    /// subscription, so nothing here has to settle them.
    /// </summary>
    private static Chapter ChapterReadyToSubmit(MockOdkContext context, Member owner)
    {
        var items = context.CreateChecklistItems();

        var chapter = context.CreateChapter(owner: owner);

        context.CreateChapterImage(chapter);
        context.CreateChapterTexts(chapter, descriptionHtml: "<p>What we do</p>");

        foreach (var type in new[]
        {
            ChecklistItemType.PrivacySettings,
            ChecklistItemType.Questions,
            ChecklistItemType.MemberProperties,
            ChecklistItemType.Topics
        })
        {
            context.Create(new ChapterChecklistItem
            {
                ChapterId = chapter.Id,
                ChecklistItemType = type,
                CompletedUtc = DateTime.UtcNow
            });
        }

        items.Should().Contain(x => x.Type == ChecklistItemType.SubmitForApproval);

        return chapter;
    }

    /// <summary>
    /// One step of the dashboard's checklist, which the test is asserting exists as much as anything else
    /// - a step filtered out by a securable would otherwise read as an unresolved one.
    /// </summary>
    private static ChecklistItemState ChecklistStep(
        GroupDashboardViewModel viewModel, ChecklistItemType type)
    {
        viewModel.Checklist.Should().NotBeNull();
        return viewModel.Checklist!.Items.Single(x => x.Type == type);
    }

    private static MockOdkContext CreateMockOdkContext()
    {
        var context = new MockOdkContext();

        return context;
    }

    private static IUnitOfWork CreateMockUnitOfWork(MockOdkContext? context = null) => MockUnitOfWorkFactory.Create(context);

    /// <summary>
    /// An approved, published group with the picture publication requires, owned by
    /// <paramref name="owner"/> - the state a dashboard prompt about an established group is read against.
    /// </summary>
    /* The wording is built from URLs, so the provider has to answer rather than be a bare mock - a null
       from it is the difference between a sentence and a NullReferenceException. */
    private static IUrlProviderFactory CreateMockUrlProviderFactory(Chapter chapter)
    {
        var urlProvider = new Mock<IUrlProvider>();

        urlProvider.Setup(x => x.GroupUrl(It.IsAny<Chapter>()))
            .Returns($"https://example.com/groups/{chapter.Slug}");

        urlProvider.Setup(x => x.MovedPageUrl(It.IsAny<Chapter>()))
            .Returns($"https://example.com/groups/{chapter.Slug}/moved");

        var factory = new Mock<IUrlProviderFactory>();

        factory.Setup(x => x.Create(It.IsAny<IServiceRequest>(), It.IsAny<Chapter?>()))
            .Returns(urlProvider.Object);

        return factory.Object;
    }

    private static Chapter CreatePublishedChapter(MockOdkContext context, Member owner, string name = "")
    {
        var chapter = context.CreateChapter(
            approvedUtc: DateTime.UtcNow,
            name: name,
            owner: owner,
            afterCreate: x => x.PublishedUtc = DateTime.UtcNow);

        context.CreateChapterImage(chapter);

        return chapter;
    }

    private static IImageService CreateMockImageService(bool isValidImage, byte[]? processedData = null)
    {
        var mock = new Mock<IImageService>();
        mock.Setup(x => x.IsImage(It.IsAny<byte[]>()))
            .Returns(isValidImage);
        if (processedData != null)
            mock.Setup(x => x.Process(It.IsAny<byte[]>(), It.IsAny<ImageProcessingOptions>())).Returns(processedData);
        return mock.Object;
    }

    private static IHtmlValidator CreateMockHtmlValidator()
    {
        var mock = new Mock<IHtmlValidator>();
        mock.Setup(x => x.Validate(It.IsAny<string?>(), It.IsAny<HtmlValidatorOptions>()))
            .Returns(ServiceResult.Successful());
        return mock.Object;
    }

    /// <summary>
    /// A validator that rejects any content holding <paramref name="rejected"/>, reporting it the way the
    /// real one does. Everything else passes.
    /// </summary>
    private static IHtmlValidator CreateMockHtmlValidator(string rejected)
    {
        var mock = new Mock<IHtmlValidator>();
        mock.Setup(x => x.Validate(It.IsAny<string?>(), It.IsAny<HtmlValidatorOptions>()))
            .Returns((string? html, HtmlValidatorOptions _) => html?.Contains(rejected) == true
                ? ServiceResult.Failure($"Unsupported HTML: {rejected}")
                : ServiceResult.Successful());
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
        mock.Setup(x => x.SendNewGroupEmail(
            It.IsAny<IMemberChapterServiceRequest>(), It.IsAny<Chapter>(), It.IsAny<IEnumerable<Member>>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    private static MemberChapterInvite CreateInvite(MockOdkContext context, Guid chapterId, Guid memberId)
        => context.Create(new MemberChapterInvite
        {
            ChapterId = chapterId,
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Token = Guid.NewGuid().ToString()
        });

    private static MemberChapterImport CreateUpload(MockOdkContext context, Guid chapterId)
        => context.Create(new MemberChapterImport
        {
            ChapterId = chapterId,
            CreatedUtc = DateTime.UtcNow,
            EmailAddress = "imported@example.com",
            Id = Guid.NewGuid(),
            UploadedUtc = DateTime.UtcNow
        });

    private static ChapterAdminService CreateChapterAdminService(
        MockOdkContext context,
        IHtmlValidator? htmlValidator = null,
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
        ChapterAdminServiceSettings? settings = null,
        SiteSubscriptionCooldown? siteSubscriptionCooldown = null)
    {
        var unitOfWork = CreateMockUnitOfWork(context);

        // One instance for both the service and the machine it fires, so a test verifying an email a
        // transition sends arranges the same object the step resolves.
        var memberEmails = memberEmailService ?? CreateMockMemberEmailService();

        return new ChapterAdminService(
            unitOfWork,
            new EmailValidationService(new InconclusiveEmailVerifier()),
            htmlValidator ?? CreateMockHtmlValidator(),
            socialMediaService ?? new Mock<ISocialMediaService>().Object,
            notificationService ?? new Mock<INotificationService>().Object,
            imageService ?? CreateMockImageService(isValidImage: true),
            memberEmails,
            topicService ?? CreateMockTopicService(),
            settings ?? CreateChapterAdminServiceSettings(),
            siteSubscriptionCooldown ?? new SiteSubscriptionCooldown(months: 0),
            siteSubscriptionService ?? new Mock<ISiteSubscriptionService>().Object,
            urlProviderFactory ?? new Mock<IUrlProviderFactory>().Object,
            paymentProviderFactory ?? new Mock<IPaymentProviderFactory>().Object,
            paymentService ?? new Mock<IPaymentService>().Object,
            geolocationService ?? CreateMockGeolocationService(country: context.CreateCountry()),
            loggingService ?? new Mock<ILoggingService>().Object,
            // The real one, not a mock: it has no dependencies and is a pure function over the arranged
            // subscription features. A bare mock returns false from every check, which silently turns any
            // feature-gated path into "not permitted" and makes the arrangement look broken instead.
            new AuthorizationService(),
            CreatePublicationRunner(unitOfWork, memberEmails));
    }

    /// <summary>
    /// The publication machine wired the way the app wires it, over the same unit of work. Its steps come from
    /// the definition, so one added later needs no change here.
    /// </summary>
    private static StateMachineRunner<
        ChapterPublicationState, ChapterPublicationTrigger, ChapterPublicationContext> CreatePublicationRunner(
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService)
    {
        var definition = ChapterPublicationStateMachine.Create();

        var services = new ServiceCollection()
            .AddSingleton(unitOfWork)
            .AddSingleton(memberEmailService)
            .AddSingleton(definition)
            .AddScoped<
                IStateResolver<ChapterPublicationState, ChapterPublicationContext>,
                ChapterPublicationStateResolver>()
            .AddScoped<
                IStepFactory<ChapterPublicationContext>,
                ServiceProviderStepFactory<ChapterPublicationContext>>()
            .AddScoped<StateMachineRunner<
                ChapterPublicationState, ChapterPublicationTrigger, ChapterPublicationContext>>();

        foreach (var stepType in definition.StepTypes)
        {
            services.AddScoped(stepType);
        }

        return services
            .BuildServiceProvider()
            .GetRequiredService<StateMachineRunner<
                ChapterPublicationState, ChapterPublicationTrigger, ChapterPublicationContext>>();
    }

    private static ChapterAdminServiceSettings CreateChapterAdminServiceSettings(
        string? defaultCountryCode = null,
        IReadOnlyCollection<string>? reservedSlugs = null) =>
        new ChapterAdminServiceSettings
        {
            ContactMessageRecaptchaScoreThreshold = 0.5,
            DashboardNewestMemberCount = 4,
            DashboardUpcomingEventCount = 3,
            DefaultCountryCode = defaultCountryCode ?? "",
            MigrationWindowDays = MigrationWindowDays,
            ReservedSlugs = reservedSlugs ?? []
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
            .Returns(platform ?? PlatformType.GroupSquirrel);

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
            .Returns(platform ?? PlatformType.GroupSquirrel);

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
        IReadOnlyCollection<Guid>? topicIds = null)
        => new ChapterCreateModel
        {
            Name = name ?? "Test Chapter",
            LocationName = locationName ?? "London",
            Location = location ?? new LatLong { Lat = 51.5, Long = -0.1 },
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
            AnswerHtml = answer ?? "<p>Test AnswerHtml</p>"
        };

    private static ChapterQuestionCreateModel CreateChapterQuestionCreateModel(
        string? name = null,
        string? answer = null)
        => new ChapterQuestionCreateModel
        {
            Name = name ?? "Test Question",
            AnswerHtml = answer ?? "<p>Test AnswerHtml</p>"
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

    private static ChapterMembershipSettingsUpdateModel CreateMembershipSettingsUpdateModel(
        bool approveNewMembers) => new()
    {
        ApproveNewMembers = approveNewMembers,
        Enabled = true,
        MembershipDisabledAfterDaysExpired = 0,
        MembershipExpiringWarningDays = 0,
        TrialPeriodMonths = 0
    };

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

    private static ChapterTexts CreateChapterTexts(
        Chapter chapter,
        string? descriptionHtml = null,
        string? shortDescription = null)
        => new ChapterTexts
        {
            ChapterId = chapter.Id,
            DescriptionHtml = descriptionHtml ?? "Test description",
            ShortDescription = shortDescription,
            WelcomeTextHtml = "Welcome to the test chapter",
            RegisterTextHtml = "Register here"
        };

    /// <summary>
    /// Defaults to what <see cref="CreateChapterTexts" /> stores, so a test that overrides nothing submits
    /// the group's texts unchanged.
    /// </summary>
    private static ChapterTextsUpdateModel CreateChapterTextsUpdateModel(
        string? descriptionHtml = null,
        string? registerTextHtml = null,
        string? shortDescription = null,
        string? welcomeTextHtml = null)
        => new ChapterTextsUpdateModel
        {
            DescriptionHtml = descriptionHtml ?? "Test description",
            RegisterTextHtml = registerTextHtml ?? "Register here",
            ShortDescription = shortDescription ?? "A group worth joining",
            WelcomeTextHtml = welcomeTextHtml ?? "Welcome to the test chapter"
        };

    private static ChapterLinks CreateChapterLinks(Chapter chapter)
        => new ChapterLinks
        {
            ChapterId = chapter.Id,
            FacebookName = null,
            InstagramName = null,
            TwitterName = null
        };

    private static ChapterPrivacySettings CreateChapterPrivacySettings(Chapter chapter)
        => new ChapterPrivacySettings { ChapterId = chapter.Id };
}
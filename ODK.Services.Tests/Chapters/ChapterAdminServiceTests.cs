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
using ODK.Core.Emails;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Resources.Resources;
using ODK.Services.Authorization;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
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
    public static async Task GetGroupDashboardViewModel_WhenGroupHasNoPicture_RequiresOne()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        var service = CreateChapterAdminService(context);

        var request = CreateMemberChapterAdminServiceRequest(
            chapter: chapter,
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert - the group is not approved, so the picture is outstanding on its own merit.
        result.NeedsImage.Should().BeTrue();
        result.NeedsImageToPublish.Should().BeFalse();
        result.CanPublish.Should().BeFalse();
        result.HasRequiredActions.Should().BeTrue();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenApprovedGroupHasNoPicture_SaysItBlocksPublication()
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
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.NeedsImage.Should().BeTrue();
        result.NeedsImageToPublish.Should().BeTrue();
        result.CanPublish.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenPublishedGroupHasNoPicture_StillRequiresOne()
    {
        // Arrange
        using var context = CreateMockOdkContext();

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

        // Assert - publishing has already happened, so the picture is outstanding without blocking it.
        result.NeedsImage.Should().BeTrue();
        result.NeedsImageToPublish.Should().BeFalse();
    }

    [Test]
    public static async Task GetGroupDashboardViewModel_WhenApprovedGroupHasAPicture_CanPublish()
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
            currentMember: currentMember);

        // Act
        var result = await service.GetGroupDashboardViewModel(request);

        // Assert
        result.CanPublish.Should().BeTrue();
        result.NeedsImage.Should().BeFalse();
        result.NeedsImageToPublish.Should().BeFalse();
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
    public static async Task GetGroupDashboardViewModel_ReturnsUpcomingEventsSoonestFirst()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(owner: currentMember);

        var venue = context.CreateVenue(chapter);

        context.CreateEvent(chapter, venue, date: DateTime.UtcNow.AddDays(-1));
        context.CreateEvent(chapter, venue, date: DateTime.UtcNow.AddDays(4));
        context.CreateEvent(chapter, venue, date: DateTime.UtcNow.AddDays(1));
        context.CreateEvent(chapter, venue, date: DateTime.UtcNow.AddDays(3));
        context.CreateEvent(chapter, venue, date: DateTime.UtcNow.AddDays(2));

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
    public static async Task GetChapterThemeViewModel_WithThemeFeature_CanEdit()
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

        // Act
        var result = await service.GetChapterThemeViewModel(request);

        // Assert
        result.CanEdit.Should().BeTrue();
    }

    [Test]
    public static async Task GetChapterThemeViewModel_WithoutThemeFeature_CannotEdit()
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
            securable: ChapterAdminSecurable.Branding);

        // Act
        var result = await service.GetChapterThemeViewModel(request);

        // Assert
        result.CanEdit.Should().BeFalse();
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

    private static MockOdkContext CreateMockOdkContext()
    {
        var context = new MockOdkContext();

        context.Add(new SiteEmailSettings { Platform = PlatformType.Default });

        return context;
    }

    private static IUnitOfWork CreateMockUnitOfWork(MockOdkContext? context = null) => MockUnitOfWorkFactory.Create(context);

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
        mock.Setup(x => x.SendNewGroupEmail(It.IsAny<IMemberChapterServiceRequest>(), It.IsAny<IEnumerable<Member>>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

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
        return new ChapterAdminService(
            unitOfWork,
            new EmailValidationService(new InconclusiveEmailVerifier()),
            htmlValidator ?? CreateMockHtmlValidator(),
            socialMediaService ?? new Mock<ISocialMediaService>().Object,
            notificationService ?? new Mock<INotificationService>().Object,
            imageService ?? CreateMockImageService(isValidImage: true),
            memberEmailService ?? CreateMockMemberEmailService(),
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
            CreatePublicationRunner(unitOfWork));
    }

    /// <summary>
    /// The publication machine wired the way the app wires it, over the same unit of work. Its steps come from
    /// the definition, so one added later needs no change here.
    /// </summary>
    private static StateMachineRunner<
        ChapterPublicationState, ChapterPublicationTrigger, ChapterPublicationContext> CreatePublicationRunner(
        IUnitOfWork unitOfWork)
    {
        var definition = ChapterPublicationStateMachine.Create();

        var services = new ServiceCollection()
            .AddSingleton(unitOfWork)
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

    private static ChapterTexts CreateChapterTexts(Chapter chapter)
        => new ChapterTexts
        {
            ChapterId = chapter.Id,
            DescriptionHtml = "Test description",
            WelcomeTextHtml = "Welcome to the test chapter",
            RegisterTextHtml = "Register here"
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
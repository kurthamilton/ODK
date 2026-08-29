using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Workflows;
using ODK.Data.Core;
using ODK.Services.Chapters;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Chapters.Workflows;
using ODK.Services.Members;
using ODK.Services.Subscriptions;
using ODK.Services.Tests.Helpers;
using ODK.Services.Workflows;

namespace ODK.Services.Tests.Chapters;

[Parallelizable]
public static class ChapterSiteAdminServiceTests
{
    [Test]
    public static async Task ApproveChapter_UnapprovedChapter_ApprovesItAndTellsTheOwner()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var owner = context.CreateMember();
        var chapter = context.CreateChapter(owner: owner);

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateService(context, emailService.Object);

        // Act
        var result = await service.ApproveChapter(SiteAdminRequest(context), chapter.Id);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Chapter>().Single(x => x.Id == chapter.Id).ApprovedUtc.Should().NotBeNull();

        emailService.Verify(
            x => x.SendGroupApprovedEmail(It.IsAny<IChapterServiceRequest>(), It.IsAny<Member>()),
            Times.Once);
    }

    [Test]
    public static async Task ApproveChapter_AlreadyApproved_SucceedsWithoutTellingTheOwnerAgain()
    {
        /* Arrange - approving twice is not a mistake, so it reports success. The machine expresses that as an
           Approve edge out of every state where only the one out of Draft does any work, which is why nothing
           here has to check first. */
        using var context = CreateMockOdkContext();
        var owner = context.CreateMember();
        var approvedUtc = DateTime.UtcNow.AddDays(-7);
        var chapter = context.CreateChapter(owner: owner, approvedUtc: approvedUtc);

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateService(context, emailService.Object);

        // Act
        var result = await service.ApproveChapter(SiteAdminRequest(context), chapter.Id);

        // Assert
        result.Success.Should().BeTrue();

        // The original date stands: approving again neither re-dates it nor re-emails.
        context.Set<Chapter>().Single(x => x.Id == chapter.Id).ApprovedUtc.Should().Be(approvedUtc);
        emailService.Verify(
            x => x.SendGroupApprovedEmail(It.IsAny<IChapterServiceRequest>(), It.IsAny<Member>()),
            Times.Never);
    }

    [Test]
    public static async Task GetSiteAdminChapterViewModel_OffersUsableSubscriptionsAndTheCurrentOne()
    {
        /* Arrange - the owner is on a subscription that has since stopped being usable, alongside a free one
           and one that is neither free nor priced. */
        using var context = CreateMockOdkContext();
        var unusableCurrent = context.CreateSiteSubscription();
        var free = context.CreateSiteSubscription(free: true);
        var unusable = context.CreateSiteSubscription();
        var owner = context.CreateMember();
        var chapter = context.CreateChapter(owner: owner, siteSubscription: unusableCurrent);

        var service = CreateService(context, Mock.Of<IMemberEmailService>());

        // Act
        var result = await service.GetSiteAdminChapterViewModel(SiteAdminChapterRequest(context, chapter));

        // Assert
        result.SiteSubscriptions.Select(x => x.Id).Should().BeEquivalentTo(new[] { unusableCurrent.Id, free.Id });
        result.SiteSubscriptions.Should().NotContain(x => x.Id == unusable.Id);
    }

    [Test]
    public static async Task UpdateSiteAdminChapter_FreeSubscription_SetsNoExpiry()
    {
        // Arrange - an owner on a paid subscription, moved onto a free one with a date still in the form.
        using var context = CreateMockOdkContext();
        var paid = context.CreateSiteSubscription();
        var free = context.CreateSiteSubscription(free: true);
        var owner = context.CreateMember();
        var chapter = context.CreateChapter(owner: owner, siteSubscription: paid);

        var service = CreateService(context, Mock.Of<IMemberEmailService>());

        // Act
        var result = await service.UpdateSiteAdminChapter(
            SiteAdminChapterRequest(context, chapter),
            new SiteAdminChapterUpdateViewModel
            {
                SiteSubscriptionId = free.Id,
                SubscriptionExpiresUtc = DateTime.UtcNow.AddYears(1)
            });

        // Assert
        result.Success.Should().BeTrue();

        var current = context.Set<MemberSiteSubscriptionRecord>()
            .Single(x => x.MemberId == owner.Id && x.IsCurrent);
        current.SiteSubscriptionId.Should().Be(free.Id);
        current.ExpiresUtc.Should().BeNull();
    }

    [Test]
    public static async Task UpdateSiteAdminChapter_PaidSubscription_SetsTheExpiry()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var paid = context.CreateSiteSubscription();
        var owner = context.CreateMember();
        var chapter = context.CreateChapter(owner: owner, siteSubscription: paid);
        var expiresUtc = new DateTime(2027, 4, 1, 12, 0, 0, DateTimeKind.Utc);

        var service = CreateService(context, Mock.Of<IMemberEmailService>());

        // Act
        var result = await service.UpdateSiteAdminChapter(
            SiteAdminChapterRequest(context, chapter),
            new SiteAdminChapterUpdateViewModel
            {
                SiteSubscriptionId = paid.Id,
                SubscriptionExpiresUtc = expiresUtc
            });

        // Assert
        result.Success.Should().BeTrue();

        var current = context.Set<MemberSiteSubscriptionRecord>()
            .Single(x => x.MemberId == owner.Id && x.IsCurrent);
        current.SiteSubscriptionId.Should().Be(paid.Id);
        current.ExpiresUtc.Should().Be(expiresUtc);
    }

    [Test]
    public static async Task UpdateSiteAdminChapter_UnusableSubscription_Fails()
    {
        // Arrange - a subscription that is neither free nor priced is not one an owner can be put on.
        using var context = CreateMockOdkContext();
        var free = context.CreateSiteSubscription(free: true);
        var unusable = context.CreateSiteSubscription();
        var owner = context.CreateMember();
        var chapter = context.CreateChapter(owner: owner, siteSubscription: free);

        var service = CreateService(context, Mock.Of<IMemberEmailService>());

        // Act
        var result = await service.UpdateSiteAdminChapter(
            SiteAdminChapterRequest(context, chapter),
            new SiteAdminChapterUpdateViewModel
            {
                SiteSubscriptionId = unusable.Id,
                SubscriptionExpiresUtc = null
            });

        // Assert
        result.Success.Should().BeFalse();

        var current = context.Set<MemberSiteSubscriptionRecord>()
            .Single(x => x.MemberId == owner.Id && x.IsCurrent);
        current.SiteSubscriptionId.Should().Be(free.Id);
    }

    private static ChapterSiteAdminService CreateService(
        MockOdkContext context, IMemberEmailService memberEmailService)
    {
        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        return new ChapterSiteAdminService(
            unitOfWork,
            new MemberSiteSubscriptionWriter(unitOfWork),
            CreatePublicationRunner(unitOfWork, memberEmailService),
            TestPaymentSettings.Create());
    }

    /// <summary>
    /// The publication machine wired the way the app wires it, over the same unit of work and email service the
    /// service under test uses. Its steps come from the definition, so one added later needs no change here.
    /// </summary>
    private static StateMachineRunner<
        ChapterPublicationState, ChapterPublicationTrigger, ChapterPublicationContext> CreatePublicationRunner(
        IUnitOfWork unitOfWork, IMemberEmailService memberEmailService)
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

    private static MockOdkContext CreateMockOdkContext() => new();

    private static IMemberChapterServiceRequest SiteAdminChapterRequest(MockOdkContext context, Chapter chapter)
    {
        var siteAdmin = context.CreateMember(afterCreate: x => x.SiteAdmin = true);

        return Mock.Of<IMemberChapterServiceRequest>(x =>
            x.Platform == PlatformType.Default &&
            x.Chapter == chapter &&
            x.CurrentMember == siteAdmin);
    }

    private static IMemberServiceRequest SiteAdminRequest(MockOdkContext context)
    {
        var siteAdmin = context.CreateMember(afterCreate: x => x.SiteAdmin = true);

        return Mock.Of<IMemberServiceRequest>(x =>
            x.Platform == PlatformType.Default &&
            x.CurrentMember == siteAdmin);
    }
}

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

    private static ChapterSiteAdminService CreateService(
        MockOdkContext context, IMemberEmailService memberEmailService)
    {
        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        return new ChapterSiteAdminService(
            unitOfWork,
            new MemberSiteSubscriptionWriter(unitOfWork),
            CreatePublicationRunner(unitOfWork, memberEmailService));
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

    private static IMemberServiceRequest SiteAdminRequest(MockOdkContext context)
    {
        var siteAdmin = context.CreateMember(afterCreate: x => x.SiteAdmin = true);

        return Mock.Of<IMemberServiceRequest>(x =>
            x.Platform == PlatformType.Default &&
            x.CurrentMember == siteAdmin);
    }
}

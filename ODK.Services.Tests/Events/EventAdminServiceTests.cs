using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Events;
using ODK.Services.Events.Models;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Notifications;
using ODK.Services.Payments;
using ODK.Services.Security;
using ODK.Services.Tasks;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Events;

[Parallelizable]
public static class EventAdminServiceTests
{
    [TestCase("Pacific Standard Time", "2024-01-17", "2024-01-15 20:00:00")]
    [TestCase("Pacific Standard Time", "2024-07-17", "2024-07-15 19:00:00")]
    [TestCase("GMT Standard Time", "2024-01-16", "2024-01-15 12:00:00")]
    [TestCase("GMT Standard Time", "2024-07-16", "2024-07-15 11:00:00")]
    [TestCase("AUS Eastern Standard Time", "2024-01-16", "2024-01-15 01:00:00")]
    [TestCase("AUS Eastern Standard Time", "2024-07-16", "2024-07-15 02:00:00")]
    public static async Task CreateEvent_ScheduledEmailDateConvertedToUtc(string timeZoneId,
        string eventDateString, string expectedScheduledEmailDate)
    {
        // Arrange
        var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            timeZone: TimeZoneInfo.FindSystemTimeZoneById(timeZoneId),
            adminMembers: [currentMember]);

        var venue = context.CreateVenue(chapter);

        context.Create(new ChapterEventSettings
        {
            ChapterId = chapter.Id,
            DefaultScheduledEmailDayOfWeek = DayOfWeek.Monday,
            DefaultScheduledEmailTimeOfDay = new TimeSpan(12, 0, 0)
        });

        var eventDate = DateTime.ParseExact(eventDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        var service = CreateService(context);

        var request = CreateMockMemberChapterAdminServiceRequest(
            securable: ChapterAdminSecurable.Events,
            currentMember: currentMember,
            chapter: chapter);

        var model = new EventCreateModel
        {
            AttendeeLimit = null,
            Date = eventDate,
            Description = null,
            EndTime = null,
            Hosts = [],
            ImageUrl = null,
            IsPublic = false,
            Name = "Name",
            RsvpDeadline = null,
            RsvpDisabled = false,
            TicketCost = null,
            TicketDepositCost = null,
            Time = null,
            VenueId = venue.Id
        };

        // Act
        await service.CreateEvent(request, model, false);

        // Assert
        var eventEmail = context.Set<EventEmail>().Single();
        var expectedScheduledUtc = DateTime.ParseExact(expectedScheduledEmailDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        eventEmail.ScheduledUtc.Should().Be(expectedScheduledUtc);
    }

    [Test]
    public static async Task SendEventInvites_UpdatesSentDate()
    {
        // Arrange
        var context = CreateMockOdkContext();

        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember],
            members: [currentMember]);

        var @event = context.CreateEvent(chapter);

        var eventEmail = context.Create(CreateEventEmail(@event));

        var service = CreateService(context);

        var request = CreateMockMemberChapterAdminServiceRequest(
            securable: ChapterAdminSecurable.Events,
            currentMember: currentMember,
            chapter: chapter);

        // Act
        await service.SendEventInvites(request, @event.Id);

        // Assert
        eventEmail = context.Set<EventEmail>().Single(x => x.Id == eventEmail.Id);
        eventEmail.SentUtc.Should().NotBeNull();
    }

    [Test]
    public static async Task SendScheduledEmails_UpdatesSentDate()
    {
        // Arrange
        var context = CreateMockOdkContext();

        var (currentMember, owner) = (context.CreateMember(), context.CreateMember());

        var chapter = context.CreateChapter(
            owner: owner,
            siteSubscription: context.CreateSiteSubscription(features: [SiteFeatureType.ScheduledEventEmails]),
            adminMembers: [currentMember],
            members: [currentMember]);

        var @event = context.CreateEvent(chapter);

        var eventEmail = context.Create(CreateEventEmail(@event));

        var service = CreateService(context);

        var request = CreateMockServiceRequest();

        // Act
        await service.SendScheduledEmails(request, eventEmail.Id);

        // Assert
        eventEmail = context.Set<EventEmail>().Single(x => x.Id == eventEmail.Id);
        eventEmail.SentUtc.Should().NotBeNull();
    }

    private static EventEmail CreateEventEmail(
        Event @event)
    {
        return new EventEmail
        {
            EventId = @event.Id,
            ScheduledUtc = DateTime.UtcNow.AddSeconds(-1)
        };
    }

    private static IMemberChapterAdminServiceRequest CreateMockMemberChapterAdminServiceRequest(
        Member currentMember,
        Chapter chapter,
        ChapterAdminSecurable? securable = null)
    {
        var mock = new Mock<IMemberChapterAdminServiceRequest>();

        mock.Setup(x => x.Chapter)
            .Returns(chapter);

        mock.Setup(x => x.CurrentMember)
            .Returns(currentMember);

        mock.Setup(x => x.Platform)
            .Returns(PlatformType.Default);

        mock.Setup(x => x.Securable)
            .Returns(securable ?? ChapterAdminSecurable.Any);

        return mock.Object;
    }

    private static MockOdkContext CreateMockOdkContext()
    {
        var context = new MockOdkContext();

        context.Add(new SiteEmailSettings { Platform = PlatformType.Default });

        return context;
    }

    private static IServiceRequest CreateMockServiceRequest()
    {
        var mock = new Mock<IServiceRequest>();

        mock.Setup(x => x.Platform)
            .Returns(PlatformType.Default);

        return mock.Object;
    }

    private static IUnitOfWork CreateMockUnitOfWork(MockOdkContext? context = null) => MockUnitOfWork.Create(context);

    private static EventAdminService CreateService(
        MockOdkContext context)
    {
        return new EventAdminService(
            unitOfWork: CreateMockUnitOfWork(context),
            Mock.Of<IAuthorizationService>(),
            Mock.Of<INotificationService>(),
            Mock.Of<IHtmlSanitizer>(),
            Mock.Of<IMemberEmailService>(),
            Mock.Of<IBackgroundTaskService>(),
            Mock.Of<ILoggingService>(),
            Mock.Of<IPaymentService>(),
            Mock.Of<IEventService>(),
            new EventAdminServiceSettings
            {
                ShortcodeLength = 8
            });
    }
}
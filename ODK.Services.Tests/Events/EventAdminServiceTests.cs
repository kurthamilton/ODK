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
using ODK.Data.Core.Events;
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

    [Test]
    public static async Task GetEventsAdminPageViewModel_DateRangeFilter_ReturnsOnlyEventsInRange()
    {
        // Arrange - a UTC chapter so local dates equal UTC dates.
        var context = CreateMockOdkContext();
        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(adminMembers: [currentMember], timeZone: TimeZoneInfo.Utc);
        var venue = context.CreateVenue(chapter);
        context.CreateEvent(chapter, venue, new DateTime(2024, 1, 10, 12, 0, 0, DateTimeKind.Utc)); // in range
        context.CreateEvent(chapter, venue, new DateTime(2024, 1, 20, 12, 0, 0, DateTimeKind.Utc)); // after To
        context.CreateEvent(chapter, venue, new DateTime(2023, 12, 1, 12, 0, 0, DateTimeKind.Utc)); // before From

        var service = CreateService(context);
        var request = CreateMockMemberChapterAdminServiceRequest(
            currentMember: currentMember, chapter: chapter, securable: ChapterAdminSecurable.Events);
        var filter = new EventAdminFilter
        {
            FromDate = new DateTime(2024, 1, 1),
            ToDate = new DateTime(2024, 1, 15)
        };

        // Act
        var result = await service.GetEventsAdminPageViewModel(request, chapter, filter, 1, 20);

        // Assert
        result.Events.TotalCount.Should().Be(1);
        result.Events.Items.Should().ContainSingle()
            .Which.Event.Date.Should().Be(new DateTime(2024, 1, 10, 12, 0, 0, DateTimeKind.Utc));
    }

    [Test]
    public static async Task GetEventsAdminPageViewModel_DateRangeFilter_UsesChapterLocalDate()
    {
        // Arrange - Pacific chapter (UTC-8 in January). The To bound is applied to the event's LOCAL date,
        // so an event stored in UTC that falls on the previous local day is included, and a naive UTC
        // comparison would wrongly exclude it.
        var context = CreateMockOdkContext();
        var currentMember = context.CreateMember();
        var pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        var chapter = context.CreateChapter(adminMembers: [currentMember], timeZone: pacific);
        var venue = context.CreateVenue(chapter);
        // 03:00 UTC on the 11th = 19:00 on the 10th in Pacific -> local date is the 10th (within To).
        var inRange = context.CreateEvent(chapter, venue, new DateTime(2024, 1, 11, 3, 0, 0, DateTimeKind.Utc));
        // 20:00 UTC on the 11th = 12:00 on the 11th in Pacific -> local date is the 11th (after To).
        context.CreateEvent(chapter, venue, new DateTime(2024, 1, 11, 20, 0, 0, DateTimeKind.Utc));

        var service = CreateService(context);
        var request = CreateMockMemberChapterAdminServiceRequest(
            currentMember: currentMember, chapter: chapter, securable: ChapterAdminSecurable.Events);
        var filter = new EventAdminFilter { ToDate = new DateTime(2024, 1, 10) };

        // Act
        var result = await service.GetEventsAdminPageViewModel(request, chapter, filter, 1, 20);

        // Assert
        result.Events.TotalCount.Should().Be(1);
        result.Events.Items.Should().ContainSingle().Which.Event.Id.Should().Be(inRange.Id);
    }

    [Test]
    public static async Task GetEventsAdminPageViewModel_Paging_ReturnsRequestedPageAndTotalCount()
    {
        // Arrange - 3 events, page size 2.
        var context = CreateMockOdkContext();
        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(adminMembers: [currentMember]);
        var venue = context.CreateVenue(chapter);
        for (var i = 0; i < 3; i++)
        {
            context.CreateEvent(chapter, venue, DateTime.UtcNow.AddDays(i));
        }

        var service = CreateService(context);
        var request = CreateMockMemberChapterAdminServiceRequest(
            currentMember: currentMember, chapter: chapter, securable: ChapterAdminSecurable.Events);

        // Act
        var page1 = await service.GetEventsAdminPageViewModel(request, chapter, new EventAdminFilter(), 1, 2);
        var page2 = await service.GetEventsAdminPageViewModel(request, chapter, new EventAdminFilter(), 2, 2);

        // Assert
        page1.Events.TotalCount.Should().Be(3);
        page1.Events.TotalPages.Should().Be(2);
        page1.Events.Items.Count.Should().Be(2);
        page2.Events.Items.Count.Should().Be(1);
    }

    [Test]
    public static async Task GetEventsAdminPageViewModel_VenueFilter_ReturnsOnlyEventsAtThatVenue()
    {
        // Arrange
        var context = CreateMockOdkContext();
        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(adminMembers: [currentMember]);
        var venue1 = context.CreateVenue(chapter);
        var venue2 = context.CreateVenue(chapter);
        context.CreateEvent(chapter, venue1);
        context.CreateEvent(chapter, venue1);
        context.CreateEvent(chapter, venue2);

        var service = CreateService(context);
        var request = CreateMockMemberChapterAdminServiceRequest(
            currentMember: currentMember, chapter: chapter, securable: ChapterAdminSecurable.Events);
        var filter = new EventAdminFilter { VenueId = venue1.Id };

        // Act
        var result = await service.GetEventsAdminPageViewModel(request, chapter, filter, 1, 20);

        // Assert
        result.Events.TotalCount.Should().Be(2);
        result.Events.Items.Should().OnlyContain(x => x.Venue.Id == venue1.Id);
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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
using ODK.Core.Venues;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.Core.Repositories;
using ODK.Services.Authorization;
using ODK.Services.Events;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Notifications;
using ODK.Services.Payments;
using ODK.Services.Tasks;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Events;

[Parallelizable]
public static class EventAdminServiceTests
{
    private static readonly Guid ChapterId = Guid.NewGuid();
    private static readonly Guid CurrentMemberId = Guid.NewGuid();
    private static readonly Guid VenueId = Guid.NewGuid();

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
        var chapter = CreateChapter();
        chapter.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        var eventDate = DateTime.ParseExact(eventDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        var chapterEventSettings = new ChapterEventSettings
        {
            ChapterId = chapter.Id,
            DefaultScheduledEmailDayOfWeek = DayOfWeek.Monday,
            DefaultScheduledEmailTimeOfDay = new TimeSpan(12, 0, 0)
        };

        var chapterEventSettingsRepository = CreateMockChapterEventSettingsRepository(chapterEventSettings);

        var chapterRepository = CreateMockChapterRepository(chapter);

        var eventEmailRepository = CreateMockEventEmailRepository();
        Mock.Get(eventEmailRepository)
            .Setup(x => x.Add(It.IsAny<EventEmail>()))
            .Callback((EventEmail eventEmail) =>
            {
                var expectedScheduledUtc = DateTime.ParseExact(expectedScheduledEmailDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                eventEmail.ScheduledUtc.Should().Be(expectedScheduledUtc);
            });

        var unitOfWork = CreateMockUnitOfWork(
            chapterRepository: chapterRepository,
            chapterEventSettingsRepository: chapterEventSettingsRepository,
            eventEmailRepository: eventEmailRepository);

        var service = CreateService(
            unitOfWork: unitOfWork);

        var request = MemberChapterServiceRequest.Create(
            ChapterId,
            CurrentMemberId,
            Mock.Of<IHttpRequestContext>(),
            PlatformType.Default);

        // Act
        await service.CreateEvent(request, new CreateEvent
        {
            AttendeeLimit = null,
            Date = eventDate,
            Description = null,
            EndTime = null,
            Hosts = new List<Guid>(),
            ImageUrl = null,
            IsPublic = false,
            Name = "Name",
            RsvpDeadline = null,
            RsvpDisabled = false,
            TicketCost = null,
            TicketDepositCost = null,
            Time = null,
            VenueId = VenueId
        }, false);

        // Assert
        Mock.Get(eventEmailRepository)
            .Verify(x => x.Add(It.IsAny<EventEmail>()), Times.Once());
    }

    private static Chapter CreateChapter()
    {
        return new Chapter
        {
            Id = ChapterId,
            Name = "Dummy",
            Slug = "slug"
        };
    }

    private static IChapterAdminMemberRepository CreateMockChapterAdminMemberRepository(
        IEnumerable<ChapterAdminMember>? chapterAdminMembers = null)
    {
        var mock = new Mock<IChapterAdminMemberRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns((Guid chapterId) => new MockDeferredQueryMultiple<ChapterAdminMember>(
                chapterAdminMembers?.Where(x => x.ChapterId == chapterId)));

        return mock.Object;
    }

    private static IChapterEventSettingsRepository CreateMockChapterEventSettingsRepository(
        ChapterEventSettings? settings = null)
    {
        var mock = new Mock<IChapterEventSettingsRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns((Guid chapterId) => new MockDeferredQuerySingleOrDefault<ChapterEventSettings>(
                settings?.ChapterId == chapterId ? settings : null));

        return mock.Object;
    }

    private static IChapterPaymentSettingsRepository CreateMockChapterPaymentSettingsRepository()
    {
        var mock = new Mock<IChapterPaymentSettingsRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingleOrDefault<ChapterPaymentSettings>(new()));

        return mock.Object;
    }

    private static IChapterRepository CreateMockChapterRepository(Chapter? chapter = null)
    {
        var mock = new Mock<IChapterRepository>();

        mock.Setup(x => x.GetById(ChapterId))
            .Returns((Guid id) => new MockDeferredQuerySingle<Chapter>(chapter?.Id == id ? chapter : null));

        return mock.Object;
    }

    private static IChapterTopicRepository CreateMockChapterTopicRepository()
    {
        var mock = new Mock<IChapterTopicRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQueryMultiple<ChapterTopic>(null));

        return mock.Object;
    }

    private static IEventEmailRepository CreateMockEventEmailRepository()
    {
        var mock = new Mock<IEventEmailRepository>();

        return mock.Object;
    }

    private static IEventHostRepository CreateMockEventHostRepository()
    {
        var mock = new Mock<IEventHostRepository>();

        return mock.Object;
    }

    private static IEventRepository CreateMockEventRepository()
    {
        var mock = new Mock<IEventRepository>();
        return mock.Object;
    }

    private static IEventTopicRepository CreateMockEventTopicRepository()
    {
        var mock = new Mock<IEventTopicRepository>();

        return mock.Object;
    }

    private static IMemberNotificationSettingsRepository CreateMockMemberNotificationSettingsRepository()
    {
        var mock = new Mock<IMemberNotificationSettingsRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>(), It.IsAny<NotificationType>()))
            .Returns(new MockDeferredQueryMultiple<MemberNotificationSettings>([]));

        return mock.Object;
    }

    private static IMemberRepository CreateMockMemberRepository()
    {
        var mock = new Mock<IMemberRepository>();

        mock.Setup(x => x.GetAllByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQueryMultiple<Member>([]));

        mock.Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingle<Member>(new Member
            {
                Chapters = new List<MemberChapter>
                {
                    new MemberChapter { ChapterId = ChapterId }
                },
                Id = CurrentMemberId
            }));

        return mock.Object;
    }

    private static IMemberSiteSubscriptionRepository CreateMockMemberSiteSubscriptionRepository()
    {
        var mock = new Mock<IMemberSiteSubscriptionRepository>();
        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingleOrDefault<MemberSiteSubscription>(null));

        return mock.Object;
    }

    private static MockUnitOfWork CreateMockUnitOfWork(
        IChapterRepository? chapterRepository = null,
        IChapterEventSettingsRepository? chapterEventSettingsRepository = null,
        IEventEmailRepository? eventEmailRepository = null)
    {
        var mock = new Mock<IUnitOfWork>();

        mock.Setup(x => x.ChapterAdminMemberRepository)
            .Returns(CreateMockChapterAdminMemberRepository([new ChapterAdminMember { ChapterId = ChapterId, MemberId = CurrentMemberId }]));

        mock.Setup(x => x.ChapterEventSettingsRepository)
            .Returns(chapterEventSettingsRepository ?? CreateMockChapterEventSettingsRepository());

        mock.Setup(x => x.ChapterPaymentSettingsRepository)
            .Returns(CreateMockChapterPaymentSettingsRepository());

        mock.Setup(x => x.ChapterRepository)
            .Returns(chapterRepository ?? CreateMockChapterRepository(CreateChapter()));

        mock.Setup(x => x.ChapterTopicRepository)
            .Returns(CreateMockChapterTopicRepository());

        mock.Setup(x => x.EventEmailRepository)
            .Returns(eventEmailRepository ?? CreateMockEventEmailRepository());

        mock.Setup(x => x.EventHostRepository)
            .Returns(CreateMockEventHostRepository());

        mock.Setup(x => x.EventRepository)
            .Returns(CreateMockEventRepository());

        mock.Setup(x => x.MemberNotificationSettingsRepository)
            .Returns(CreateMockMemberNotificationSettingsRepository());

        mock.Setup(x => x.MemberRepository)
            .Returns(CreateMockMemberRepository());

        mock.Setup(x => x.MemberSiteSubscriptionRepository)
            .Returns(CreateMockMemberSiteSubscriptionRepository());

        mock.Setup(x => x.EventTopicRepository)
            .Returns(CreateMockEventTopicRepository());

        mock.Setup(x => x.VenueRepository)
            .Returns(CreateMockVenueRepository());

        return new MockUnitOfWork(mock);
    }

    private static IVenueRepository CreateMockVenueRepository()
    {
        var mock = new Mock<IVenueRepository>();

        mock.Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns((Guid id) => new MockDeferredQuerySingle<Venue>(id == VenueId ? new Venue
            {
                ChapterId = ChapterId,
                Id = VenueId
            } : null));

        return mock.Object;
    }

    private static EventAdminService CreateService(
        IUnitOfWork? unitOfWork = null)
    {
        return new EventAdminService(
            unitOfWork: unitOfWork ?? CreateMockUnitOfWork(),
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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Events;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Core.Venues;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.Core.Repositories;
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
        var chapter = CreateChapter(
            timeZone: TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        var currentMember = CreateMember(id: CurrentMemberId);

        var venue = CreateVenue();
        var venueRepository = CreateMockVenueRepository([venue]);

        var adminMember = CreateChapterAdminMember(
            chapterId: chapter.Id,
            member: currentMember);
        var adminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

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
            chapterAdminMemberRepository: adminMemberRepository,
            chapterRepository: chapterRepository,
            chapterEventSettingsRepository: chapterEventSettingsRepository,
            eventEmailRepository: eventEmailRepository,
            venueRepository: venueRepository);

        var service = CreateService(
            unitOfWork: unitOfWork);

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
        };

        // Act
        await service.CreateEvent(request, model, false);

        // Assert
        Mock.Get(eventEmailRepository)
            .Verify(x => x.Add(It.IsAny<EventEmail>()), Times.Once());
    }

    [Test]
    public static async Task SendEventInvites_UpdatesSentDate()
    {
        // Arrange
        var chapter = CreateChapter();
        var currentMember = CreateMember(id: CurrentMemberId, chapter: chapter);

        var adminMember = CreateChapterAdminMember(
            chapterId: chapter.Id,
            member: currentMember);
        var adminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var @event = CreateEvent(
            chapterId: chapter.Id);
        var eventRepository = CreateMockEventRepository([@event]);

        var eventEmail = CreateEventEmail(eventId: @event.Id);
        var eventEmailRepository = CreateMockEventEmailRepository([eventEmail]);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: adminMemberRepository,
            eventRepository: eventRepository,
            eventEmailRepository: eventEmailRepository);

        var service = CreateService(
            unitOfWork: unitOfWork);

        var request = CreateMockMemberChapterAdminServiceRequest(
            securable: ChapterAdminSecurable.Events,
            currentMember: currentMember,
            chapter: chapter);

        // Act
        await service.SendEventInvites(request, @event.Id);

        // Assert
        eventEmail.SentUtc.Should().NotBeNull();
        Mock.Get(eventEmailRepository)
            .Verify(x => x.Update(eventEmail), Times.Once);
        unitOfWork.Mock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public static async Task SendScheduledEmails_UpdatesSentDate()
    {
        // Arrange
        var chapter = CreateChapter(
            ownerId: Guid.NewGuid());
        var currentMember = CreateMember(id: CurrentMemberId, chapter: chapter);

        var adminMember = CreateChapterAdminMember(
            chapterId: chapter.Id,
            member: currentMember);
        var adminMemberRepository = CreateMockChapterAdminMemberRepository([adminMember]);

        var venue = CreateVenue();
        var venueRepository = CreateMockVenueRepository([venue]);

        var @event = CreateEvent(
            chapterId: chapter.Id);
        var eventRepository = CreateMockEventRepository([@event]);

        var eventEmail = CreateEventEmail(eventId: @event.Id);
        var eventEmailRepository = CreateMockEventEmailRepository([eventEmail]);

        var ownerSubscription = CreateMemberSiteSubscription(
            memberId: chapter.OwnerId,
            features: [SiteFeatureType.ScheduledEventEmails]);
        var memberSiteSubscriptionRepository = CreateMockMemberSiteSubscriptionRepository(
            byChapter: ownerSubscription);

        var unitOfWork = CreateMockUnitOfWork(
            chapterAdminMemberRepository: adminMemberRepository,
            eventRepository: eventRepository,
            eventEmailRepository: eventEmailRepository,
            venueRepository: venueRepository,
            memberSiteSubscriptionRepository: memberSiteSubscriptionRepository);

        var service = CreateService(
            unitOfWork: unitOfWork);

        var request = CreateMockServiceRequest();

        // Act
        await service.SendScheduledEmails(request, eventEmail.Id);

        // Assert
        eventEmail.SentUtc.Should().NotBeNull();
        Mock.Get(eventEmailRepository)
            .Verify(x => x.Update(eventEmail), Times.Once);
        unitOfWork.Mock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    private static Chapter CreateChapter(
        TimeZoneInfo? timeZone = null,
        Guid? ownerId = null)
    {
        return new Chapter
        {
            Id = ChapterId,
            Name = "Dummy",
            OwnerId = ownerId ?? Guid.NewGuid(),
            Slug = "slug",
            TimeZone = timeZone ?? TimeZoneInfo.Utc
        };
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

    private static Event CreateEvent(
        Guid? id = null,
        Guid? chapterId = null,
        DateTime? date = null)
    {
        return new Event
        {
            ChapterId = chapterId ?? Guid.NewGuid(),
            Date = date ?? DateTime.UtcNow.AddDays(5),
            Id = id ?? Guid.NewGuid(),
            PublishedUtc = DateTime.UtcNow,
            VenueId = VenueId
        };
    }

    private static EventEmail CreateEventEmail(
        Guid? eventId = null)
    {
        return new EventEmail
        {
            EventId = eventId ?? Guid.NewGuid(),
            ScheduledUtc = DateTime.UtcNow.AddSeconds(-1)
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

    private static MemberSiteSubscription CreateMemberSiteSubscription(
        Guid? memberId = null,
        IEnumerable<SiteFeatureType>? features = null)
    {
        return new MemberSiteSubscription
        {
            MemberId = memberId ?? Guid.NewGuid(),
            SiteSubscription = CreateSiteSubscription(
                features: features)
        };
    }

    private static IChapterAdminMemberRepository CreateMockChapterAdminMemberRepository(
        IEnumerable<ChapterAdminMember>? adminMembers = null)
    {
        var mock = new Mock<IChapterAdminMemberRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<PlatformType>(), It.IsAny<Guid>()))
            .Returns((PlatformType platform, Guid chapterId) => new MockDeferredQueryMultiple<ChapterAdminMember>(
                adminMembers?.Where(x => x.ChapterId == chapterId)));

        mock.Setup(x => x.GetByMemberId(It.IsAny<PlatformType>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns((PlatformType platform, Guid memberId, Guid chapterId) =>
                new MockDeferredQuerySingleOrDefault<ChapterAdminMember>(
                    adminMembers?.FirstOrDefault(x => x.ChapterId == chapterId && x.MemberId == memberId)));

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

    private static IChapterMembershipSettingsRepository CreateMockChapterMembershipSettingsRepository()
    {
        var mock = new Mock<IChapterMembershipSettingsRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingleOrDefault<ChapterMembershipSettings>(null));

        return mock.Object;
    }

    private static IChapterPaymentSettingsRepository CreateMockChapterPaymentSettingsRepository()
    {
        var mock = new Mock<IChapterPaymentSettingsRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingleOrDefault<ChapterPaymentSettings>(new()));

        return mock.Object;
    }

    private static IChapterPrivacySettingsRepository CreateMockChapterPrivacySettingsRepository()
    {
        var mock = new Mock<IChapterPrivacySettingsRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingleOrDefault<ChapterPrivacySettings>(null));

        return mock.Object;
    }

    private static IChapterRepository CreateMockChapterRepository(Chapter? chapter = null)
    {
        var mock = new Mock<IChapterRepository>();

        mock.Setup(x => x.GetById(It.IsAny<PlatformType>(), It.IsAny<Guid>()))
            .Returns((PlatformType platform, Guid id)
                => new MockDeferredQuerySingle<Chapter>(chapter?.Id == id ? chapter : null));

        return mock.Object;
    }

    private static IChapterTopicRepository CreateMockChapterTopicRepository()
    {
        var mock = new Mock<IChapterTopicRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQueryMultiple<ChapterTopic>(null));

        return mock.Object;
    }

    private static ICurrencyRepository CreateMockCurrencyRepository()
    {
        var mock = new Mock<ICurrencyRepository>();
        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingle<Currency>(new Currency
            {
                Code = "",
                Symbol = "$",
                Id = Guid.NewGuid()
            }));
        return mock.Object;
    }

    private static IEventEmailRepository CreateMockEventEmailRepository(
        IEnumerable<EventEmail>? eventEmails = null)
    {
        var mock = new Mock<IEventEmailRepository>();

        mock.Setup(x => x.GetByEventId(It.IsAny<Guid>()))
            .Returns((Guid eventId) => new MockDeferredQuerySingleOrDefault<EventEmail>(
                eventEmails?.FirstOrDefault(x => x.EventId == eventId)));

        mock.Setup(x => x.GetByIdOrDefault(It.IsAny<Guid>()))
            .Returns((Guid id) => new MockDeferredQuerySingleOrDefault<EventEmail>(
                eventEmails?.FirstOrDefault(x => x.Id == id)));

        return mock.Object;
    }

    private static IEventHostRepository CreateMockEventHostRepository()
    {
        var mock = new Mock<IEventHostRepository>();

        return mock.Object;
    }

    private static IEventInviteRepository CreateMockEventInviteRepository()
    {
        var mock = new Mock<IEventInviteRepository>();

        mock.Setup(x => x.GetByEventId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQueryMultiple<EventInvite>([]));

        return mock.Object;
    }

    private static IEventRepository CreateMockEventRepository(
        IEnumerable<Event>? events = null)
    {
        var mock = new Mock<IEventRepository>();

        mock.Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns((Guid id) => new MockDeferredQuerySingle<Event>(events?.FirstOrDefault(x => x.Id == id)));

        mock.Setup(x => x.ShortcodeExists(It.IsAny<string>()))
            .Returns(new MockDeferredQuery<bool>(false));

        return mock.Object;
    }

    private static IEventResponseRepository CreateMockEventResponseRepository()
    {
        var mock = new Mock<IEventResponseRepository>();

        mock.Setup(x => x.GetByEventId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQueryMultiple<EventResponse>([]));

        return mock.Object;
    }

    private static IEventTopicRepository CreateMockEventTopicRepository()
    {
        var mock = new Mock<IEventTopicRepository>();

        return mock.Object;
    }

    private static IMemberChapterAdminServiceRequest CreateMockMemberChapterAdminServiceRequest(
        Member? currentMember = null,
        Chapter? chapter = null,
        ChapterAdminSecurable? securable = null)
    {
        var mock = new Mock<IMemberChapterAdminServiceRequest>();

        mock.Setup(x => x.Chapter)
            .Returns(chapter ?? CreateChapter());

        mock.Setup(x => x.CurrentMember)
            .Returns(currentMember ?? CreateMember(id: CurrentMemberId, chapter: chapter));

        mock.Setup(x => x.Securable)
            .Returns(securable ?? ChapterAdminSecurable.Any);

        return mock.Object;
    }

    private static IMemberEmailPreferenceRepository CreateMockMemberEmailPreferenceRepository()
    {
        var mock = new Mock<IMemberEmailPreferenceRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>(), MemberEmailPreferenceType.Events))
            .Returns(new MockDeferredQueryMultiple<MemberEmailPreference>([]));

        mock.Setup(x => x.GetByMemberId(It.IsAny<Guid>(), MemberEmailPreferenceType.Events))
            .Returns(new MockDeferredQuerySingleOrDefault<MemberEmailPreference>(null));

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

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
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

    private static IMemberSiteSubscriptionRepository CreateMockMemberSiteSubscriptionRepository(
        MemberSiteSubscription? byChapter = null)
    {
        var mock = new Mock<IMemberSiteSubscriptionRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingleOrDefault<MemberSiteSubscription>(byChapter));

        return mock.Object;
    }

    private static IMemberSubscriptionRepository CreateMockMemberSubscriptionRepository()
    {
        var mock = new Mock<IMemberSubscriptionRepository>();

        mock.Setup(x => x.GetByChapterId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQueryMultiple<MemberSubscription>([]));

        return mock.Object;
    }

    private static IServiceRequest CreateMockServiceRequest()
    {
        var mock = new Mock<IServiceRequest>();

        return mock.Object;
    }

    private static MockUnitOfWork CreateMockUnitOfWork(
        IChapterRepository? chapterRepository = null,
        IChapterEventSettingsRepository? chapterEventSettingsRepository = null,
        IEventRepository? eventRepository = null,
        IEventEmailRepository? eventEmailRepository = null,
        IChapterAdminMemberRepository? chapterAdminMemberRepository = null,
        IEventResponseRepository? eventResponseRepository = null,
        IEventInviteRepository? eventInviteRepository = null,
        IVenueRepository? venueRepository = null,
        IMemberSiteSubscriptionRepository? memberSiteSubscriptionRepository = null)
    {
        var mock = new Mock<IUnitOfWork>();

        mock.Setup(x => x.ChapterAdminMemberRepository)
            .Returns(chapterAdminMemberRepository ?? CreateMockChapterAdminMemberRepository(
                [new ChapterAdminMember { ChapterId = ChapterId, MemberId = CurrentMemberId, Role = ChapterAdminRole.Admin }]));

        mock.Setup(x => x.ChapterEventSettingsRepository)
            .Returns(chapterEventSettingsRepository ?? CreateMockChapterEventSettingsRepository());

        mock.Setup(x => x.ChapterMembershipSettingsRepository)
            .Returns(CreateMockChapterMembershipSettingsRepository());

        mock.Setup(x => x.ChapterPaymentSettingsRepository)
            .Returns(CreateMockChapterPaymentSettingsRepository());

        mock.Setup(x => x.ChapterPrivacySettingsRepository)
            .Returns(CreateMockChapterPrivacySettingsRepository());

        mock.Setup(x => x.ChapterRepository)
            .Returns(chapterRepository ?? CreateMockChapterRepository(CreateChapter()));

        mock.Setup(x => x.ChapterTopicRepository)
            .Returns(CreateMockChapterTopicRepository());

        mock.Setup(x => x.CurrencyRepository)
            .Returns(CreateMockCurrencyRepository());

        mock.Setup(x => x.EventEmailRepository)
            .Returns(eventEmailRepository ?? CreateMockEventEmailRepository());

        mock.Setup(x => x.EventHostRepository)
            .Returns(CreateMockEventHostRepository());

        mock.Setup(x => x.EventInviteRepository)
            .Returns(eventInviteRepository ?? CreateMockEventInviteRepository());

        mock.Setup(x => x.EventRepository)
            .Returns(eventRepository ?? CreateMockEventRepository());

        mock.Setup(x => x.EventResponseRepository)
            .Returns(eventResponseRepository ?? CreateMockEventResponseRepository());

        mock.Setup(x => x.EventTopicRepository)
            .Returns(CreateMockEventTopicRepository());

        mock.Setup(x => x.MemberEmailPreferenceRepository)
            .Returns(CreateMockMemberEmailPreferenceRepository());

        mock.Setup(x => x.MemberNotificationSettingsRepository)
            .Returns(CreateMockMemberNotificationSettingsRepository());

        mock.Setup(x => x.MemberRepository)
            .Returns(CreateMockMemberRepository());

        mock.Setup(x => x.MemberSiteSubscriptionRepository)
            .Returns(memberSiteSubscriptionRepository ?? CreateMockMemberSiteSubscriptionRepository());

        mock.Setup(x => x.MemberSubscriptionRepository)
            .Returns(CreateMockMemberSubscriptionRepository());

        mock.Setup(x => x.VenueRepository)
            .Returns(venueRepository ?? CreateMockVenueRepository());

        return new MockUnitOfWork(mock);
    }

    private static IVenueRepository CreateMockVenueRepository(
        IEnumerable<Venue>? venues = null)
    {
        var mock = new Mock<IVenueRepository>();

        mock.Setup(x => x.GetByEventId(It.IsAny<Guid>()))
            .Returns(new MockDeferredQuerySingle<Venue>(CreateVenue()));

        mock.Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns((Guid id) => new MockDeferredQuerySingle<Venue>(
                venues?.FirstOrDefault(x => x.Id == id)));

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

    private static SiteSubscription CreateSiteSubscription(
        IEnumerable<SiteFeatureType>? features = null)
    {
        return new SiteSubscription
        {
            Features = features?.Select(x => new SiteSubscriptionFeature
            {
                Feature = x
            }).ToArray() ?? []
        };
    }

    private static Venue CreateVenue(
        Guid? chapterId = null)
    {
        return new Venue
        {
            ChapterId = chapterId ?? ChapterId,
            Id = VenueId
        };
    }
}
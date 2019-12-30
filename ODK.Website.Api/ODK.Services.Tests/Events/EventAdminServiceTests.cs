using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Mail;
using ODK.Core.Members;
using ODK.Core.Venues;
using ODK.Services.Events;
using ODK.Services.Mails;

namespace ODK.Services.Tests.Events
{
    public static class EventAdminServiceTests
    {
        private const string BaseUrl = "http://mock.com";
        private const string EventRsvpUrlFormat = "/{event.id}?rsvp=yes";
        private const string EventUrlFormat = "/{event.id}";

        [Test]
        public static async Task GetEventEmail_ReplacesChapterProperties()
        {
            Chapter chapter = CreateMockChapter("Chapter");
            IChapterRepository chapterRepository = CreateMockChapterRepository(chapter: chapter);

            Email email = new Email(EmailType.EventInvite, "Subject: {chapter.name}", "Body: {chapter.name}");
            IEmailRepository emailRepository = CreateMockEmailRepository(email);

            EventAdminService service = CreateService(emailRepository, chapterRepository);

            Email eventEmail = await service.GetEventEmail(Guid.NewGuid(), Guid.NewGuid());

            Assert.AreEqual("Subject: Chapter", eventEmail.Subject);
            Assert.AreEqual("Body: Chapter", eventEmail.HtmlContent);
        }

        [Test]
        public static async Task GetEventEmail_ReplacesEventProperties()
        {
            Venue venue = new Venue(Guid.Empty, Guid.NewGuid(), "Location", "", "", 0);
            IVenueRepository venueRepository = CreateMockVenueRepository(venue);

            Event @event = CreateMockEvent("Name", new DateTime(2015, 6, 7), "Time");
            IEventRepository eventRepository = CreateMockEventRepository(@event);

            Email email = new Email(EmailType.EventInvite, "Subject: {event.name}", "Name: {event.name}, Date: {event.date}, Location: {event.location}, Time: {event.time}");
            IEmailRepository memberEmailRepository = CreateMockEmailRepository(email);

            EventAdminService service = CreateService(memberEmailRepository, eventRepository: eventRepository,
                venueRepository: venueRepository);

            Email eventEmail = await service.GetEventEmail(Guid.NewGuid(), Guid.NewGuid());

            Assert.AreEqual("Subject: Name", eventEmail.Subject);
            Assert.AreEqual("Name: Name, Date: Sunday 07 June, 2015, Location: Location, Time: Time", eventEmail.HtmlContent);
        }

        [Test]
        public static async Task GetEventEmail_ReplacesEventUrlProperties()
        {
            Event @event = CreateMockEvent("Name", new DateTime(2015, 6, 7), "Time");
            IEventRepository eventRepository = CreateMockEventRepository(@event);

            Email email = new Email(EmailType.EventInvite, "Subject: {event.url}", "RSVP: {event.rsvpurl}, Url: {event.url}");
            IEmailRepository memberEmailRepository = CreateMockEmailRepository(email);

            EventAdminService service = CreateService(memberEmailRepository, eventRepository: eventRepository);

            Email eventEmail = await service.GetEventEmail(Guid.NewGuid(), Guid.NewGuid());

            Assert.AreEqual($"Subject: {BaseUrl}/{@event.Id}", eventEmail.Subject);
            Assert.AreEqual($"RSVP: {BaseUrl}/{@event.Id}?rsvp=yes, Url: {BaseUrl}/{@event.Id}" , eventEmail.HtmlContent);
        }

        private static EventAdminService CreateService(IEmailRepository memberEmailRepository = null,
            IChapterRepository chapterRepository = null, IEventRepository eventRepository = null,
            IVenueRepository venueRepository = null)
        {
            return new EventAdminService(
                eventRepository ?? CreateMockEventRepository(CreateMockEvent()),
                chapterRepository ?? CreateMockChapterRepository(chapter: CreateMockChapter()),
                memberEmailRepository ?? CreateMockEmailRepository(new Email(EmailType.EventInvite, "Subject", "Body")),
                new EventAdminServiceSettings { BaseUrl = BaseUrl, EventRsvpUrlFormat = EventRsvpUrlFormat, EventUrlFormat = EventUrlFormat },
                GetMockMemberRepository(),
                venueRepository ?? CreateMockVenueRepository(new Venue(Guid.NewGuid(), Guid.NewGuid(), "Name", "Address", "", 0)),
                Mock.Of<IMailProviderFactory>());
        }

        private static Chapter CreateMockChapter(string name = null)
        {
            return new Chapter(Guid.NewGuid(), Guid.NewGuid(), name ?? "Chapter", "", "Welcome text", null, 1);
        }

        private static IChapterRepository CreateMockChapterRepository(bool authorised = true, Chapter chapter = null)
        {
            Mock<IChapterRepository> mock = new Mock<IChapterRepository>();

            mock.Setup(x => x.GetChapter(It.IsAny<Guid>()))
                .ReturnsAsync(chapter);

            mock.Setup(x => x.GetChapterAdminMember(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(authorised ? new ChapterAdminMember(Guid.NewGuid(), Guid.NewGuid()) : null);

            return mock.Object;
        }

        private static Event CreateMockEvent(string name = null, DateTime? date = null, string time = null)
        {
            return new Event(Guid.NewGuid(), Guid.NewGuid(), "Admin Member", name ?? "Name", date ?? DateTime.Today, Guid.NewGuid(),
                time ?? "Time", null, "Description", false);
        }

        private static IEventRepository CreateMockEventRepository(Event @event = null)
        {
            Mock<IEventRepository> mock = new Mock<IEventRepository>();

            mock.Setup(x => x.GetEvent(It.IsAny<Guid>()))
                .ReturnsAsync(@event);

            return mock.Object;
        }

        private static IEmailRepository CreateMockEmailRepository(Email email = null)
        {
            Mock<IEmailRepository> mock = new Mock<IEmailRepository>();

            mock.Setup(x => x.GetEmail(It.IsAny<EmailType>()))
                .ReturnsAsync(email);

            mock.Setup(x => x.GetEmail(It.IsAny<EmailType>(), It.IsAny<Guid>()))
                .ReturnsAsync(email);

            return mock.Object;
        }

        private static IMemberRepository GetMockMemberRepository()
        {
            return Mock.Of<IMemberRepository>();
        }

        private static IVenueRepository CreateMockVenueRepository(Venue venue = null)
        {
            Mock<IVenueRepository> mock = new Mock<IVenueRepository>();

            mock.Setup(x => x.GetVenue(It.IsAny<Guid>()))
                .ReturnsAsync(venue);

            return mock.Object;
        }
    }
}

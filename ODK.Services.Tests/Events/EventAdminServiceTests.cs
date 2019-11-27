using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Mail;
using ODK.Services.Events;

namespace ODK.Services.Tests.Events
{
    public static class EventAdminServiceTests
    {
        private const string BaseUrl = "http://mock.com";
        private const string EventRsvpUrlFormat = "/{event.id}/rsvp/{token}";
        private const string EventUrlFormat = "/{event.id}";

        [Test]
        public static async Task GetEventEmail_ReplacesChapterProperties()
        {
            Chapter chapter = CreateMockChapter("Chapter");
            IChapterRepository chapterRepository = CreateMockChapterRepository(chapter: chapter);

            Email email = new Email(EmailType.EventInvite, "Subject: {chapter.name}", "Body: {chapter.name}");
            IMemberEmailRepository memberEmailRepository = CreateMockMemberEmailRepository(email);

            EventAdminService service = CreateService(memberEmailRepository, chapterRepository);

            Email eventEmail = await service.GetEventEmail(Guid.NewGuid(), Guid.NewGuid());

            Assert.AreEqual("Subject: Chapter", eventEmail.Subject);
            Assert.AreEqual("Body: Chapter", eventEmail.Body);
        }

        [Test]
        public static async Task GetEventEmail_ReplacesEventProperties()
        {
            Event @event = CreateMockEvent("Name", new DateTime(2015, 6, 7), "Location", "Time");
            IEventRepository eventRepository = CreateMockEventRepository(@event);

            Email email = new Email(EmailType.EventInvite, "Subject: {event.name}", "Name: {event.name}, Date: {event.date}, Location: {event.location}, Time: {event.time}");
            IMemberEmailRepository memberEmailRepository = CreateMockMemberEmailRepository(email);

            EventAdminService service = CreateService(memberEmailRepository, eventRepository: eventRepository);

            Email eventEmail = await service.GetEventEmail(Guid.NewGuid(), Guid.NewGuid());

            Assert.AreEqual("Subject: Name", eventEmail.Subject);
            Assert.AreEqual("Name: Name, Date: Sunday 07 June, 2015, Location: Location, Time: Time", eventEmail.Body);
        }

        [Test]
        public static async Task GetEventEmail_ReplacesEventUrlProperties()
        {
            Event @event = CreateMockEvent("Name", new DateTime(2015, 6, 7), "Location", "Time");
            IEventRepository eventRepository = CreateMockEventRepository(@event);

            Email email = new Email(EmailType.EventInvite, "Subject: {event.url}", "RSVP: {event.rsvpurl}, Url: {event.url}");
            IMemberEmailRepository memberEmailRepository = CreateMockMemberEmailRepository(email);

            EventAdminService service = CreateService(memberEmailRepository, eventRepository: eventRepository);

            Email eventEmail = await service.GetEventEmail(Guid.NewGuid(), Guid.NewGuid());

            Assert.AreEqual($"Subject: {BaseUrl}/{@event.Id}", eventEmail.Subject);
            Assert.AreEqual($"RSVP: {BaseUrl}/{@event.Id}/rsvp/{{token}}, Url: {BaseUrl}/{@event.Id}" , eventEmail.Body);
        }

        private static EventAdminService CreateService(IMemberEmailRepository memberEmailRepository = null,
            IChapterRepository chapterRepository = null, IEventRepository eventRepository = null)
        {
            return new EventAdminService(
                eventRepository ?? CreateMockEventRepository(CreateMockEvent()),
                chapterRepository ?? CreateMockChapterRepository(chapter: CreateMockChapter()),
                memberEmailRepository ?? CreateMockMemberEmailRepository(new Email(EmailType.EventInvite, "Subject", "Body")),
                new EventAdminServiceSettings { BaseUrl = BaseUrl, EventRsvpUrlFormat = EventRsvpUrlFormat, EventUrlFormat = EventUrlFormat });
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

        private static Event CreateMockEvent(string name = null, DateTime? date = null, string location = null, string time = null)
        {
            return new Event(Guid.NewGuid(), Guid.NewGuid(), name ?? "Name", date ?? DateTime.Today, location ?? "Location",
                time ?? "Time", null, "Address", "MapQuery", "Description", false);
        }

        private static IEventRepository CreateMockEventRepository(Event @event = null)
        {
            Mock<IEventRepository> mock = new Mock<IEventRepository>();

            mock.Setup(x => x.GetEvent(It.IsAny<Guid>()))
                .ReturnsAsync(@event);

            return mock.Object;
        }

        private static IMemberEmailRepository CreateMockMemberEmailRepository(Email email = null)
        {
            Mock<IMemberEmailRepository> mock = new Mock<IMemberEmailRepository>();

            mock.Setup(x => x.GetEmail(It.IsAny<EmailType>()))
                .ReturnsAsync(email);

            return mock.Object;
        }
    }
}

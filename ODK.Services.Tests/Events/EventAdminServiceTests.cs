using System;
using Moq;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Venues;
using ODK.Services.Emails;
using ODK.Services.Events;

namespace ODK.Services.Tests.Events;

public static class EventAdminServiceTests
{
    private const string BaseUrl = "http://mock.com";
    private const string EventRsvpUrlFormat = "/{event.id}?rsvp=yes";
    private const string EventUrlFormat = "/{event.id}";

    private static EventAdminService CreateService(IEmailRepository? memberEmailRepository = null,
        IChapterRepository? chapterRepository = null, IEventRepository? eventRepository = null,
        IVenueRepository? venueRepository = null)
    {
        return new EventAdminService(
            eventRepository ?? CreateMockEventRepository(CreateMockEvent()),
            chapterRepository ?? CreateMockChapterRepository(chapter: CreateMockChapter()),
            new EventAdminServiceSettings { BaseUrl = BaseUrl, EventRsvpUrlFormat = EventRsvpUrlFormat, EventUrlFormat = EventUrlFormat },
            GetMockMemberRepository(),
            venueRepository ?? CreateMockVenueRepository(new Venue(Guid.NewGuid(), Guid.NewGuid(), "Name", "Address", "", 0)),
            Mock.Of<IEmailService>());
    }

    private static Chapter CreateMockChapter(string? name = null)
    {
        return new Chapter(Guid.NewGuid(), Guid.NewGuid(), name ?? "Chapter", "", "Welcome text", null, 1);
    }

    private static IChapterRepository CreateMockChapterRepository(bool authorised = true, Chapter? chapter = null)
    {
        Mock<IChapterRepository> mock = new Mock<IChapterRepository>();

        mock.Setup(x => x.GetChapter(It.IsAny<Guid>()))
            .ReturnsAsync(chapter);

        mock.Setup(x => x.GetChapterAdminMember(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(authorised ? new ChapterAdminMember(Guid.NewGuid(), Guid.NewGuid()) : null);

        return mock.Object;
    }

    private static Event CreateMockEvent(string? name = null, DateTime? date = null, string? time = null)
    {
        return new Event(Guid.NewGuid(), Guid.NewGuid(), "Admin Member", name ?? "Name", date ?? DateTime.Today, Guid.NewGuid(),
            time ?? "Time", null, "Description", false);
    }

    private static IEventRepository CreateMockEventRepository(Event? @event = null)
    {
        Mock<IEventRepository> mock = new Mock<IEventRepository>();

        mock.Setup(x => x.GetEventAsync(It.IsAny<Guid>()))
            .ReturnsAsync(@event);

        return mock.Object;
    }

    private static IMemberRepository GetMockMemberRepository()
    {
        return Mock.Of<IMemberRepository>();
    }

    private static IVenueRepository CreateMockVenueRepository(Venue? venue = null)
    {
        Mock<IVenueRepository> mock = new Mock<IVenueRepository>();

        mock.Setup(x => x.GetVenueAsync(It.IsAny<Guid>()))
            .ReturnsAsync(venue);

        return mock.Object;
    }
}

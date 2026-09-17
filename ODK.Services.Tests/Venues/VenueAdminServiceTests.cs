using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Venues;
using ODK.Services.Geolocation;
using ODK.Services.Places;
using ODK.Services.Security;
using ODK.Services.Tests.Helpers;
using ODK.Services.Venues;
using ODK.Services.Venues.Models;

namespace ODK.Services.Tests.Venues;

[Parallelizable]
public static class VenueAdminServiceTests
{
    private const string OakExternalId = "ChIJoak";

    [Test]
    public static async Task ArchiveVenue_NotThisChaptersVenue_Throws()
    {
        // Arrange - no link joins the venue to this chapter, so it is not this chapter's to archive.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var otherVenue = context.CreateVenue(context.CreateChapter(), "The Oak", "the-oak");
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var act = async () => await service.ArchiveVenue(request, otherVenue.Id);

        // Assert
        await act.Should().ThrowAsync<OdkNotFoundException>();
    }

    [Test]
    public static async Task ArchiveVenue_RecordsTheArchiveOnTheChapterLink()
    {
        // Arrange - archiving is per chapter, so the link is what has to carry it.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var venue = context.CreateVenue(chapter, "The Oak", "the-oak");
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.ArchiveVenue(request, venue.Id);

        // Assert
        result.Success.Should().BeTrue();
        ChapterLink(context, chapter, venue).ArchivedUtc.Should().NotBeNull();
    }

    [Test]
    public static async Task CreateVenue_AlreadyLinkedToThisChapter_Fails()
    {
        // Arrange - the chapter already uses this place, so there is nothing to add.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        context.CreateVenue(chapter, "The Oak", "the-oak", externalId: OakExternalId);
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel());

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("You already have this venue");
    }

    [Test]
    public static async Task CreateVenue_ComparesTheStoredPositionWithThePlace()
    {
        // Arrange - the threshold is only as good as what it is handed, and a location's LatLong is
        // derived from two columns. A pair built from one of them twice reads as a plausible position and
        // would put every existing venue hundreds of kilometres from its own place, so the comparison has
        // to be shown the coordinates that were actually stored.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        context.CreateVenue(
            context.CreateChapter(), "The Oak", "the-oak-sheffield",
            externalId: OakExternalId, lat: 53.0, @long: -1.0);

        var comparisons = new List<(LatLong Stored, LatLong Place)>();
        var (service, request) = CreateService(
            context, currentMember, chapter, comparisons: comparisons);

        // Act
        await service.CreateVenue(request, CreateModel());

        // Assert
        comparisons.Should().ContainSingle();

        var (stored, place) = comparisons[0];
        stored.Lat.Should().Be(53.0);
        stored.Long.Should().Be(-1.0);
        place.Lat.Should().Be(53.3811);
        place.Long.Should().Be(-1.4701);
    }

    [Test]
    public static async Task CreateVenue_ExistingVenueUntracked_LinksWithoutReinsertingIt()
    {
        // Arrange - the real context reads without tracking, so the venue a link is built from arrives
        // detached. Setting the navigation on a new link makes EF treat that venue as part of the added
        // graph and insert it again, against a primary key that already exists. The other tests here run
        // with tracking, which resolves the instance and hides it.
        var (context, currentMember, chapter) = CreateChapterWithOwner(noTracking: true);
        context.CreateVenue(
            context.CreateChapter(), "The Oak", "the-oak-sheffield",
            externalId: OakExternalId, lat: 53.3811, @long: -1.4701);
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel());

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Venue>().Should().ContainSingle();
    }

    /* The boundary is inclusive, and the two cases sit either side of it by a hundredth of a metre. The
       exact figure matters less than that it is one figure - a place either moved or did not. */
    [TestCase(49.99, true)]
    [TestCase(50, true)]
    [TestCase(50.01, false)]
    public static async Task CreateVenue_DistanceEitherSideOfTheThreshold(
        double metresApart, bool expectedSameRecord)
    {
        // Arrange
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        context.CreateVenue(
            context.CreateChapter(), "The Oak", "the-oak-sheffield", externalId: OakExternalId);
        var (service, request) = CreateService(
            context, currentMember, chapter, metresApart: metresApart);

        // Act
        var result = await service.CreateVenue(request, CreateModel());

        // Assert - within the threshold the place was located more precisely, beyond it the place moved.
        result.Success.Should().BeTrue();
        context.Set<Venue>().Should().HaveCount(expectedSameRecord ? 1 : 2);
    }

    [Test]
    public static async Task CreateVenue_ExternalIdMissing_Fails()
    {
        // Arrange - the lookup is the only required field; without it there is no place to resolve.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel(externalId: null));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Location required");
    }

    [Test]
    public static async Task CreateVenue_LookupFails_Fails()
    {
        // Arrange
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var (service, request) = CreateService(
            context, currentMember, chapter, place: GetPlaceResult.Failure("Place lookup failed"));

        // Act
        var result = await service.CreateVenue(request, CreateModel());

        // Assert - nothing is written from a lookup that did not answer.
        result.Success.Should().BeFalse();
        context.Set<Venue>().Should().BeEmpty();
    }

    [Test]
    public static async Task CreateVenue_NameDiffersFromThePlace_StoresItOnTheLink()
    {
        // Arrange
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel(name: "Thursday pub"));

        // Assert - the venue keeps the place's name; the chapter's own name sits on its link.
        result.Success.Should().BeTrue();
        context.Set<Venue>().Single().Name.Should().Be("The Oak");
        context.Set<ChapterVenue>().Single().Name.Should().Be("Thursday pub");
    }

    [Test]
    public static async Task CreateVenue_NameMatchesThePlace_StoresNoNameOnTheLink()
    {
        // Arrange - a chapter that has not renamed anything follows the place, so nothing is copied.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel(name: "The Oak"));

        // Assert
        result.Success.Should().BeTrue();
        context.Set<ChapterVenue>().Single().Name.Should().BeNull();
    }

    [Test]
    public static async Task CreateVenue_NewPlace_RecordsWhatTheLookupReturned()
    {
        // Arrange
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel());

        // Assert - the venue is the place, not the submission. The slug carries the town so two pubs of
        // one name in different places do not collide.
        result.Success.Should().BeTrue();
        var venue = context.Set<Venue>().Single();
        venue.Name.Should().Be("The Oak");
        venue.Slug.Should().Be("the-oak-sheffield");

        var location = context.Set<VenueLocation>().Single();
        location.ExternalId.Should().Be(OakExternalId);
        location.Latitude.Should().Be(53.3811);
        location.Longitude.Should().Be(-1.4701);
    }

    [Test]
    public static async Task CreateVenue_PlaceMovedFar_RecordsItAgain()
    {
        // Arrange - a relocation is a different building, so the old record stands and a new one is made.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        context.CreateVenue(
            context.CreateChapter(), "The Oak", "the-oak-sheffield", externalId: OakExternalId);
        var (service, request) = CreateService(context, currentMember, chapter, metresApart: 400);

        // Act
        var result = await service.CreateVenue(request, CreateModel());

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Venue>().Should().HaveCount(2);
    }

    [Test]
    public static async Task CreateVenue_PlaceMovedSlightly_LinksTheExistingVenue()
    {
        // Arrange - a few metres is the lookup locating the same place more precisely, not a move.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var existing = context.CreateVenue(
            context.CreateChapter(), "The Oak", "the-oak-sheffield", externalId: OakExternalId);
        var (service, request) = CreateService(context, currentMember, chapter, metresApart: 8);

        // Act
        var result = await service.CreateVenue(request, CreateModel());

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Venue>().Should().ContainSingle();
        context.Set<ChapterVenue>().Single(x => x.ChapterId == chapter.Id)
            .VenueId.Should().Be(existing.Id);
    }

    [Test]
    public static async Task CreateVenue_PlaceNotFound_AsksForItAgain()
    {
        // Arrange - a place id stops resolving when the place closes, moves or is merged.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var (service, request) = CreateService(
            context, currentMember, chapter, place: GetPlaceResult.PlaceNotFound());

        // Act
        var result = await service.CreateVenue(request, CreateModel());

        // Assert - something the member can act on, so it says so.
        result.Success.Should().BeFalse();
        result.Message.Should().Be("That location could not be found - search for it again");
    }

    [Test]
    public static async Task CreateVenue_PlaceRenamed_RecordsItAgain()
    {
        // Arrange - an event held at the old name was held at the old name, so the record is not rewritten.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        context.CreateVenue(
            context.CreateChapter(), "The Old Oak", "the-old-oak-sheffield", externalId: OakExternalId);
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel());

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Venue>().Should().HaveCount(2);
        context.Set<Venue>().Select(x => x.Name).Should().Contain(["The Old Oak", "The Oak"]);
    }

    [Test]
    public static async Task CreateVenue_SamePlaceAnotherChapterUses_LinksTheExistingVenue()
    {
        // Arrange - the same place described the same way is one venue, however many chapters reach it.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var existing = context.CreateVenue(
            context.CreateChapter(), "The Oak", "the-oak-sheffield",
            externalId: OakExternalId, lat: 53.3811, @long: -1.4701);
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel());

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Venue>().Should().ContainSingle();
        context.Set<ChapterVenue>().Single(x => x.ChapterId == chapter.Id)
            .VenueId.Should().Be(existing.Id);
    }

    [Test]
    public static async Task CreateVenue_SlugAlreadyTaken_VersionsIt()
    {
        // Arrange - a different place whose name and town slug to the same value.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        context.CreateVenue(
            context.CreateChapter(), "The Oak", "the-oak-sheffield", externalId: "ChIJother");
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel());

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Venue>().Single(x => x.Id != context.Set<Venue>().First().Id)
            .Slug.Should().Be("the-oak-sheffield-2");
    }

    [Test]
    public static async Task DeleteVenue_RemovesTheVenueAndItsChapterLink()
    {
        // Arrange
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var venue = context.CreateVenue(chapter, "The Oak", "the-oak");
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.DeleteVenue(request, venue.Id);

        // Assert - the venue goes, not only this chapter's link to it.
        result.Success.Should().BeTrue();
        context.Set<Venue>().Should().NotContain(x => x.Id == venue.Id);
        context.Set<ChapterVenue>().Should().NotContain(x => x.VenueId == venue.Id);
    }

    [Test]
    public static async Task DeleteVenue_VenueHasEvents_Fails()
    {
        // Arrange
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var venue = context.CreateVenue(chapter, "The Oak", "the-oak");
        context.CreateEvent(chapter, venue);
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.DeleteVenue(request, venue.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot delete a venue with events");
        context.Set<Venue>().Should().Contain(x => x.Id == venue.Id);
    }

    [Test]
    public static async Task GetVenue_NotLinkedToThisChapter_Throws()
    {
        // Arrange - a venue that exists but which no link joins to this chapter.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var venue = context.Create(new Venue
        {
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            Name = "The Oak",
            Slug = "the-oak"
        });
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var act = async () => await service.GetVenue(request, venue.Id);

        // Assert
        await act.Should().ThrowAsync<OdkNotFoundException>();
    }

    [Test]
    public static async Task GetVenue_SharedWithAnotherChapter_IsFound()
    {
        // Arrange - a venue another chapter already uses, linked to this one as well. The link is the
        // only thing that makes it this chapter's.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var venue = context.CreateVenue(context.CreateChapter(), "The Oak", "the-oak");
        context.Create(new ChapterVenue { ChapterId = chapter.Id, VenueId = venue.Id, Venue = venue });
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.GetVenue(request, venue.Id);

        // Assert
        result.Id.Should().Be(venue.Id);
    }

    [Test]
    public static async Task RestoreVenue_ClearsTheArchiveOnTheChapterLink()
    {
        // Arrange
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var venue = context.CreateVenue(chapter, "The Oak", "the-oak", archived: true);
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.RestoreVenue(request, venue.Id);

        // Assert
        result.Success.Should().BeTrue();
        ChapterLink(context, chapter, venue).ArchivedUtc.Should().BeNull();
    }

    [Test]
    public static async Task UpdateVenue_ChangesTheLinkAndLeavesThePlaceAlone()
    {
        // Arrange
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var venue = context.CreateVenue(chapter, "The Oak", "the-oak-sheffield");
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.UpdateVenue(request, venue.Id, new VenueUpdateModel
        {
            AdditionalInfo = "Side door",
            Name = "Thursday pub"
        });

        // Assert - the venue's name and slug are the place's and are shared, so editing cannot reach them.
        result.Success.Should().BeTrue();
        var link = ChapterLink(context, chapter, venue);
        link.Name.Should().Be("Thursday pub");
        link.AdditionalInfo.Should().Be("Side door");
        context.Set<Venue>().Single().Name.Should().Be("The Oak");
        context.Set<Venue>().Single().Slug.Should().Be("the-oak-sheffield");
    }

    [Test]
    public static async Task UpdateVenue_NotThisChaptersVenue_Throws()
    {
        // Arrange - the venue exists, but no link joins it to this chapter.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var otherVenue = context.CreateVenue(context.CreateChapter(), "The Oak", "the-oak");
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var act = async () => await service.UpdateVenue(request, otherVenue.Id, new VenueUpdateModel
        {
            AdditionalInfo = null,
            Name = "Renamed"
        });

        // Assert
        await act.Should().ThrowAsync<OdkNotFoundException>();
    }

    private static ChapterVenue ChapterLink(MockOdkContext context, Chapter chapter, Venue venue)
        => context.Set<ChapterVenue>().Single(x => x.ChapterId == chapter.Id && x.VenueId == venue.Id);

    private static (MockOdkContext Context, Member CurrentMember, Chapter Chapter) CreateChapterWithOwner(
        bool noTracking = false)
    {
        var context = new MockOdkContext(noTracking);
        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(adminMembers: [currentMember]);
        return (context, currentMember, chapter);
    }

    private static VenueCreateModel CreateModel(
        string? name = null, string? externalId = OakExternalId) => new()
        {
            AdditionalInfo = null,
            ExternalId = externalId,
            Name = name
        };

    private static Place CreateOakPlace() => new()
    {
        ExternalId = OakExternalId,
        FormattedAddress = "123 High St, Sheffield S1 2AB, UK",
        Location = new LatLong(53.3811, -1.4701),
        Locality = "Sheffield",
        Name = "The Oak"
    };

    /* metresApart is what the calculator reports between an existing record and the place just resolved -
       the only thing the service asks it, and what decides whether the place has moved. */
    private static (IVenueAdminService Service, IMemberChapterAdminServiceRequest Request) CreateService(
        MockOdkContext context,
        Member currentMember,
        Chapter chapter,
        GetPlaceResult? place = null,
        double metresApart = 0,
        List<(LatLong Stored, LatLong Place)>? comparisons = null)
    {
        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        var placesService = new Mock<IPlacesService>();
        placesService
            .Setup(x => x.GetPlace(It.IsAny<string>()))
            .ReturnsAsync(place ?? GetPlaceResult.Found(CreateOakPlace()));

        var latLongCalculator = new Mock<ILatLongCalculator>();
        latLongCalculator
            .Setup(x => x.CalculateMetresBetween(It.IsAny<LatLong>(), It.IsAny<LatLong>()))
            .Callback<LatLong, LatLong>((stored, place) => comparisons?.Add((stored, place)))
            .Returns(metresApart);

        var request = new Mock<IMemberChapterAdminServiceRequest>();
        request.Setup(x => x.Chapter).Returns(chapter);
        request.Setup(x => x.CurrentMember).Returns(currentMember);
        request.Setup(x => x.Platform).Returns(PlatformType.GroupSquirrel);
        request.Setup(x => x.Securable).Returns(ChapterAdminSecurable.Venues);

        var service = new VenueAdminService(
            unitOfWork, placesService.Object, latLongCalculator.Object);

        return (service, request.Object);
    }
}

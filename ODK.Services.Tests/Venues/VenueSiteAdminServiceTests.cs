using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Venues;
using ODK.Services.Exceptions;
using ODK.Services.Places;
using ODK.Services.Tests.Helpers;
using ODK.Services.Venues;

namespace ODK.Services.Tests.Venues;

[Parallelizable]
public static class VenueSiteAdminServiceTests
{
    private const string OakExternalId = "ChIJoak";

    [Test]
    public static async Task BackfillVenue_SetsTheVenueFromThePlace()
    {
        // Arrange - a venue as an organiser typed it, before the lookup was the source of anything.
        var (context, siteAdmin) = CreateContextWithSiteAdmin();
        var venue = context.CreateVenue("The Oak, S1 2AB", "the-oak-s1-2ab");
        var (service, request) = CreateService(context, siteAdmin);

        // Act
        var result = await service.BackfillVenue(request, venue.Id, OakExternalId);

        // Assert - the details a venue created today would have taken from the same place.
        result.Success.Should().BeTrue();

        var updated = context.Set<Venue>().Single(x => x.Id == venue.Id);
        updated.Name.Should().Be("The Oak");
        updated.Slug.Should().Be("the-oak-sheffield");

        var location = context.Set<VenueLocation>().Single(x => x.VenueId == venue.Id);
        location.ExternalId.Should().Be(OakExternalId);
        location.Latitude.Should().Be(53.3811);
        location.Longitude.Should().Be(-1.4701);
        location.MapQuery.Should().Be("123 High St, Sheffield S1 2AB, UK");
    }

    [Test]
    public static async Task BackfillVenue_KeepsTheNameItHadAsEachGroupsOwn()
    {
        // Arrange - two groups on a venue whose name is what somebody typed, not what the place is called.
        var (context, siteAdmin) = CreateContextWithSiteAdmin();
        var venue = context.CreateVenue("The Oak, S1 2AB", "the-oak-s1-2ab");
        var first = context.CreateChapterVenue(context.CreateChapter(), venue);
        var second = context.CreateChapterVenue(context.CreateChapter(), venue);
        var (service, request) = CreateService(context, siteAdmin);

        // Act
        var result = await service.BackfillVenue(request, venue.Id, OakExternalId);

        // Assert - the venue takes the place's name and each group carries on seeing the old one.
        result.Success.Should().BeTrue();
        context.Set<Venue>().Single(x => x.Id == venue.Id).Name.Should().Be("The Oak");
        context.Set<ChapterVenue>().Single(x => x.Id == first.Id).Name.Should().Be("The Oak, S1 2AB");
        context.Set<ChapterVenue>().Single(x => x.Id == second.Id).Name.Should().Be("The Oak, S1 2AB");
    }

    [Test]
    public static async Task BackfillVenue_GroupAlreadyNamedIt_LeavesThatNameAlone()
    {
        // Arrange - a group that has already called the venue something of its own.
        var (context, siteAdmin) = CreateContextWithSiteAdmin();
        var venue = context.CreateVenue("The Oak, S1 2AB", "the-oak-s1-2ab");
        var chapterVenue = context.CreateChapterVenue(context.CreateChapter(), venue);
        chapterVenue.Name = "Thursday pub";
        var (service, request) = CreateService(context, siteAdmin);

        // Act
        var result = await service.BackfillVenue(request, venue.Id, OakExternalId);

        // Assert - what a group chose outranks what the venue happened to be called.
        result.Success.Should().BeTrue();
        context.Set<ChapterVenue>().Single(x => x.Id == chapterVenue.Id).Name.Should().Be("Thursday pub");
    }

    [Test]
    public static async Task BackfillVenue_NameAlreadyMatchesThePlace_LeavesTheLinksUnnamed()
    {
        // Arrange - nothing to preserve: the venue is already called what the place is called.
        var (context, siteAdmin) = CreateContextWithSiteAdmin();
        var venue = context.CreateVenue("The Oak", "the-oak");
        var chapterVenue = context.CreateChapterVenue(context.CreateChapter(), venue);
        var (service, request) = CreateService(context, siteAdmin);

        // Act
        var result = await service.BackfillVenue(request, venue.Id, OakExternalId);

        // Assert - a null keeps the group following the place, which is what it was already doing.
        result.Success.Should().BeTrue();
        context.Set<ChapterVenue>().Single(x => x.Id == chapterVenue.Id).Name.Should().BeNull();
    }

    [Test]
    public static async Task BackfillVenue_AnotherVenueIsAlreadyThePlace_Fails()
    {
        // Arrange - one venue per place, so resolving a second one onto it would need a merge.
        var (context, siteAdmin) = CreateContextWithSiteAdmin();
        context.CreateVenue("The Oak", "the-oak-sheffield", externalId: OakExternalId);
        var venue = context.CreateVenue("The Oak, S1 2AB", "the-oak-s1-2ab");
        var (service, request) = CreateService(context, siteAdmin);

        // Act
        var result = await service.BackfillVenue(request, venue.Id, OakExternalId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("'The Oak' is already this place");
        context.Set<Venue>().Single(x => x.Id == venue.Id).Name.Should().Be("The Oak, S1 2AB");
    }

    [Test]
    public static async Task BackfillVenue_AlreadyThisPlace_KeepsItsOwnSlug()
    {
        // Arrange - re-running a backfill, or correcting one detail of it.
        var (context, siteAdmin) = CreateContextWithSiteAdmin();
        var venue = context.CreateVenue("The Oak", "the-oak-sheffield", externalId: OakExternalId);
        var (service, request) = CreateService(context, siteAdmin);

        // Act
        var result = await service.BackfillVenue(request, venue.Id, OakExternalId);

        // Assert - its own slug is in the candidate set, and versioning away from itself would be absurd.
        result.Success.Should().BeTrue();
        context.Set<Venue>().Single(x => x.Id == venue.Id).Slug.Should().Be("the-oak-sheffield");
    }

    [Test]
    public static async Task BackfillVenue_NoLocationRecorded_CreatesOne()
    {
        // Arrange - a venue old enough to have no location row at all.
        var (context, siteAdmin) = CreateContextWithSiteAdmin();
        var venue = context.Create(new Venue
        {
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            Name = "The Oak, S1 2AB",
            Slug = "the-oak-s1-2ab"
        });
        var (service, request) = CreateService(context, siteAdmin);

        // Act
        var result = await service.BackfillVenue(request, venue.Id, OakExternalId);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<VenueLocation>().Single(x => x.VenueId == venue.Id)
            .ExternalId.Should().Be(OakExternalId);
    }

    [Test]
    public static async Task BackfillVenue_ExternalIdMissing_Fails()
    {
        // Arrange - nothing to resolve, so nothing to apply.
        var (context, siteAdmin) = CreateContextWithSiteAdmin();
        var venue = context.CreateVenue("The Oak, S1 2AB", "the-oak-s1-2ab");
        var (service, request) = CreateService(context, siteAdmin);

        // Act
        var result = await service.BackfillVenue(request, venue.Id, null);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Location required");
        context.Set<Venue>().Single(x => x.Id == venue.Id).Name.Should().Be("The Oak, S1 2AB");
    }

    [Test]
    public static async Task BackfillVenue_PlaceNotFound_Fails()
    {
        // Arrange - a place id stops resolving when the place closes, moves or is merged.
        var (context, siteAdmin) = CreateContextWithSiteAdmin();
        var venue = context.CreateVenue("The Oak, S1 2AB", "the-oak-s1-2ab");
        var (service, request) = CreateService(context, siteAdmin, GetPlaceResult.PlaceNotFound());

        // Act
        var result = await service.BackfillVenue(request, venue.Id, OakExternalId);

        // Assert - nothing is written from a lookup that did not answer.
        result.Success.Should().BeFalse();
        result.Message.Should().Be("That location could not be found - search for it again");
        context.Set<Venue>().Single(x => x.Id == venue.Id).Name.Should().Be("The Oak, S1 2AB");
    }

    [Test]
    public static async Task BackfillVenue_NotSiteAdmin_Throws()
    {
        // Arrange
        var context = new MockOdkContext();
        var member = context.CreateMember();
        var venue = context.CreateVenue("The Oak, S1 2AB", "the-oak-s1-2ab");
        var (service, request) = CreateService(context, member);

        // Act
        var act = async () => await service.BackfillVenue(request, venue.Id, OakExternalId);

        // Assert
        await act.Should().ThrowAsync<OdkNotAuthorizedException>();
    }

    [Test]
    public static async Task DeleteVenue_Orphan_Deletes()
    {
        // Arrange - a venue whose last link has gone, which is what the site admin page exists to tidy.
        var (context, siteAdmin) = CreateContextWithSiteAdmin();
        var venue = context.CreateVenue("The Oak", "the-oak");
        var (service, request) = CreateService(context, siteAdmin);

        // Act
        var result = await service.DeleteVenue(request, venue.Id);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Venue>().Should().NotContain(x => x.Id == venue.Id);
    }

    [Test]
    public static async Task DeleteVenue_HasEvents_Fails()
    {
        // Arrange - an event outlives the link that made it, so a venue with no links can still be the
        // place a past event was held. Deleting it would take that with it.
        var (context, siteAdmin) = CreateContextWithSiteAdmin();
        var chapter = context.CreateChapter();
        var venue = context.CreateVenue("The Oak", "the-oak");
        context.CreateEvent(chapter, context.CreateChapterVenue(chapter, venue));
        RemoveLinks(context, venue);
        var (service, request) = CreateService(context, siteAdmin);

        // Act
        var result = await service.DeleteVenue(request, venue.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot delete a venue with events");
        context.Set<Venue>().Should().Contain(x => x.Id == venue.Id);
    }

    [Test]
    public static async Task DeleteVenue_StillLinkedToAGroup_Fails()
    {
        // Arrange - the count is what makes a venue deletable, and a linked one is in use.
        var (context, siteAdmin) = CreateContextWithSiteAdmin();
        var venue = context.CreateVenue("The Oak", "the-oak");
        context.CreateChapterVenue(context.CreateChapter(), venue);
        var (service, request) = CreateService(context, siteAdmin);

        // Act
        var result = await service.DeleteVenue(request, venue.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot delete a venue a group is using");
        context.Set<Venue>().Should().Contain(x => x.Id == venue.Id);
    }

    [Test]
    public static async Task DeleteVenue_NotSiteAdmin_Throws()
    {
        // Arrange
        var context = new MockOdkContext();
        var member = context.CreateMember();
        var venue = context.CreateVenue("The Oak", "the-oak");
        var (service, request) = CreateService(context, member);

        // Act
        var act = async () => await service.DeleteVenue(request, venue.Id);

        // Assert
        await act.Should().ThrowAsync<OdkNotAuthorizedException>();
    }

    [Test]
    public static async Task GetVenuesViewModel_CountsTheGroupsUsingEachVenue()
    {
        // Arrange - the count is the number that says whether venues are being shared at all.
        var (context, siteAdmin) = CreateContextWithSiteAdmin();
        var shared = context.CreateVenue("The Oak", "the-oak");
        context.CreateChapterVenue(context.CreateChapter(), shared);
        context.CreateChapterVenue(context.CreateChapter(), shared);

        var orphan = context.CreateVenue("The Elm", "the-elm");

        var (service, request) = CreateService(context, siteAdmin);

        // Act
        var result = await service.GetVenuesViewModel(request);

        // Assert
        result.Venues.Single(x => x.Venue.Id == shared.Id).ChapterCount.Should().Be(2);
        result.Venues.Single(x => x.Venue.Id == orphan.Id).ChapterCount.Should().Be(0);
    }

    private static (MockOdkContext Context, Member SiteAdmin) CreateContextWithSiteAdmin()
    {
        var context = new MockOdkContext();
        return (context, context.CreateMember(siteAdmin: true));
    }

    private static Place CreateOakPlace() => new()
    {
        ExternalId = OakExternalId,
        FormattedAddress = "123 High St, Sheffield S1 2AB, UK",
        Location = new LatLong(53.3811, -1.4701),
        Locality = "Sheffield",
        Name = "The Oak"
    };

    private static (IVenueSiteAdminService Service, IMemberServiceRequest Request) CreateService(
        MockOdkContext context, Member currentMember, GetPlaceResult? place = null)
    {
        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        var placesService = new Mock<IPlacesService>();
        placesService
            .Setup(x => x.GetPlace(It.IsAny<string>()))
            .ReturnsAsync(place ?? GetPlaceResult.Found(CreateOakPlace()));

        var request = new Mock<IMemberServiceRequest>();
        request.Setup(x => x.CurrentMember).Returns(currentMember);

        // The real slug rules, because what a backfill produces is the whole point of these tests.
        var service = new VenueSiteAdminService(unitOfWork, placesService.Object, new VenueSlugService());

        return (service, request.Object);
    }

    /* An event is made through a link, so a venue with events but no links has to be arranged in that
       order rather than declared. */
    private static void RemoveLinks(MockOdkContext context, Venue venue)
    {
        // The helpers only track what they create, so the rows have to exist before they can be removed.
        context.SaveChanges();

        foreach (var link in context.Set<ChapterVenue>().Where(x => x.VenueId == venue.Id).ToArray())
        {
            context.Remove(link);
        }

        context.SaveChanges();
    }
}

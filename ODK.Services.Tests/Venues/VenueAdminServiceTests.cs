using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Exceptions;
using ODK.Services.Exceptions;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Services.Security;
using ODK.Services.Tests.Helpers;
using ODK.Services.Venues;
using ODK.Services.Venues.Models;

namespace ODK.Services.Tests.Venues;

[Parallelizable]
public static class VenueAdminServiceTests
{
    [Test]
    public static async Task BackfillSlugs_LeavesExistingSlugsAloneAndAvoidsThem()
    {
        // Arrange
        // The case the deferred backfill creates: a venue created after the column shipped already
        // holds "the-oak", so the legacy venue whose name also slugs to "the-oak" must be versioned
        // around it rather than duplicating it.
        var (context, currentMember, chapter) = CreateChapterWithOwner(siteAdmin: true);
        context.CreateVenue(chapter, "The Oak!", "the-oak");
        context.CreateVenue(chapter, "The Oak");
        var service = CreateService(context);

        // Act
        var result = await service.BackfillSlugs(CreateMemberServiceRequest(currentMember));

        // Assert
        result.Success.Should().BeTrue();
        VenueNamed(context, "The Oak!").Slug.Should().Be("the-oak");
        VenueNamed(context, "The Oak").Slug.Should().Be("the-oak-2");
    }

    [Test]
    public static async Task BackfillSlugs_NotSiteAdmin_Throws()
    {
        // Arrange
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        context.CreateVenue(chapter, "The Oak");
        var service = CreateService(context);

        // Act
        var act = async () => await service.BackfillSlugs(CreateMemberServiceRequest(currentMember));

        // Assert
        await act.Should().ThrowAsync<OdkNotAuthorizedException>();
    }

    [Test]
    public static async Task BackfillSlugs_RunTwice_ChangesNothingTheSecondTime()
    {
        // Arrange - the button is expected to be pressed more than once.
        var (context, currentMember, chapter) = CreateChapterWithOwner(siteAdmin: true);
        context.CreateVenue(chapter, "The Oak");
        context.CreateVenue(chapter, "The Oak!");
        var service = CreateService(context);

        // Act
        await service.BackfillSlugs(CreateMemberServiceRequest(currentMember));
        var first = context.Set<Venue>().ToDictionary(x => x.Name, x => x.Slug);
        var result = await service.BackfillSlugs(CreateMemberServiceRequest(currentMember));

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Venue>().ToDictionary(x => x.Name, x => x.Slug).Should().BeEquivalentTo(first);
    }

    [Test]
    public static async Task BackfillSlugs_SlugsAreScopedPerChapter()
    {
        // Arrange - the same venue name in two chapters keeps the same slug in each.
        var (context, currentMember, _) = CreateChapterWithOwner(siteAdmin: true);
        context.CreateVenue(context.CreateChapter(), "The Oak");
        context.CreateVenue(context.CreateChapter(), "The Oak");
        var service = CreateService(context);

        // Act
        await service.BackfillSlugs(CreateMemberServiceRequest(currentMember));

        // Assert
        context.Set<Venue>()
            .Where(x => x.Name == "The Oak")
            .Select(x => x.Slug)
            .Should().AllBe("the-oak");
    }

    [Test]
    public static async Task BackfillSlugs_UnsluggableName_LeavesSlugNull()
    {
        // Arrange - nothing sluggable, so there is no slug to give it; the column stays nullable.
        var (context, currentMember, chapter) = CreateChapterWithOwner(siteAdmin: true);
        context.CreateVenue(chapter, "!!!");
        var service = CreateService(context);

        // Act
        var result = await service.BackfillSlugs(CreateMemberServiceRequest(currentMember));

        // Assert
        result.Success.Should().BeTrue();
        VenueNamed(context, "!!!").Slug.Should().BeNull();
    }

    [Test]
    public static async Task CreateVenue_DuplicateName_Fails()
    {
        // Arrange
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        context.CreateVenue(chapter, "The Oak", "the-oak");
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel("The Oak"));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Venue with that name already exists");
    }

    [Test]
    public static async Task CreateVenue_DuplicateNameDifferingOnlyByWhitespace_Fails()
    {
        // Arrange
        // A venue stored before names were normalised. The database's unique index treats "The  Oak"
        // and "The Oak" as distinct, so nothing stops the insert - the duplicate has to be caught here
        // or the two compete for one slug.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        context.CreateVenue(chapter, "The  Oak", "the-oak");
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel("The Oak"));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Venue with that name already exists");
    }

    [Test]
    public static async Task CreateVenue_NameHasStrayWhitespace_StoresItNormalised()
    {
        // Arrange
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel("  The   Oak  "));

        // Assert
        result.Success.Should().BeTrue();
        var venue = SingleVenue(context, chapter);
        venue.Name.Should().Be("The Oak");
        venue.Slug.Should().Be("the-oak");
    }

    [Test]
    public static async Task CreateVenue_SetsSlugFromName()
    {
        // Arrange
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel("The Oak & Acorn"));

        // Assert
        result.Success.Should().BeTrue();
        SingleVenue(context, chapter).Slug.Should().Be("the-oak-and-acorn");
    }

    [Test]
    public static async Task CreateVenue_SlugTakenByAnotherVenue_VersionsTheSlug()
    {
        // Arrange
        // Names are unique per chapter, so a slug collision can only come from two different names
        // that slug to the same value - here the trailing "!" is dropped by the slug rules.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        context.CreateVenue(chapter, "The Oak", "the-oak");
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel("The Oak!"));

        // Assert
        result.Success.Should().BeTrue();
        VenueNamed(context, "The Oak!").Slug.Should().Be("the-oak-2");
    }

    [Test]
    public static async Task CreateVenue_SlugTakenInAnotherChapter_DoesNotVersion()
    {
        // Arrange - slugs are unique per chapter, so another chapter's slug is not a collision.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        context.CreateVenue(context.CreateChapter(), "The Oak", "the-oak");
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.CreateVenue(request, CreateModel("The Oak"));

        // Assert
        result.Success.Should().BeTrue();
        SingleVenue(context, chapter).Slug.Should().Be("the-oak");
    }

    [Test]
    public static async Task UpdateVenue_NotThisChaptersVenue_Throws()
    {
        // Arrange - the venue exists, but belongs to a different chapter.
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var otherVenue = context.CreateVenue(context.CreateChapter(), "The Oak", "the-oak");
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var act = async () => await service.UpdateVenue(request, otherVenue.Id, CreateModel("Renamed"));

        // Assert
        await act.Should().ThrowAsync<OdkNotFoundException>();
    }

    [Test]
    public static async Task UpdateVenue_RenamedToAnotherFormOfItsOwnName_KeepsItsSlug()
    {
        // Arrange - the venue's new name slugs to the slug it already holds. Excluding itself from the
        // taken set is what stops it colliding with itself and versioning to "the-oak-2".
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var venue = context.CreateVenue(chapter, "The Oak", "the-oak");
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.UpdateVenue(request, venue.Id, CreateModel("The Oak!"));

        // Assert
        result.Success.Should().BeTrue();
        VenueNamed(context, "The Oak!").Slug.Should().Be("the-oak");
    }

    [Test]
    public static async Task UpdateVenue_Renamed_UpdatesTheSlug()
    {
        // Arrange
        var (context, currentMember, chapter) = CreateChapterWithOwner();
        var venue = context.CreateVenue(chapter, "The Oak", "the-oak");
        var (service, request) = CreateService(context, currentMember, chapter);

        // Act
        var result = await service.UpdateVenue(request, venue.Id, CreateModel("The Elm"));

        // Assert
        result.Success.Should().BeTrue();
        VenueNamed(context, "The Elm").Slug.Should().Be("the-elm");
    }

    private static (MockOdkContext Context, Member CurrentMember, Chapter Chapter) CreateChapterWithOwner(
        bool siteAdmin = false)
    {
        var context = new MockOdkContext();
        var currentMember = context.CreateMember(siteAdmin: siteAdmin);
        var chapter = context.CreateChapter(adminMembers: [currentMember]);
        return (context, currentMember, chapter);
    }

    private static IMemberServiceRequest CreateMemberServiceRequest(Member currentMember)
    {
        var request = new Mock<IMemberServiceRequest>();
        request.Setup(x => x.CurrentMember).Returns(currentMember);
        request.Setup(x => x.CurrentMemberOrDefault).Returns(currentMember);
        request.Setup(x => x.Platform).Returns(PlatformType.Default);
        return request.Object;
    }

    private static VenueCreateModel CreateModel(string name) => new()
    {
        Address = null,
        Location = new LatLong(51.5074, -0.1278),
        LocationName = "London",
        Name = name
    };

    private static IVenueAdminService CreateService(MockOdkContext context)
        => new VenueAdminService(MockUnitOfWork.Create(context));

    private static (IVenueAdminService Service, IMemberChapterAdminServiceRequest Request) CreateService(
        MockOdkContext context, Member currentMember, Chapter chapter)
    {
        var unitOfWork = MockUnitOfWork.Create(context);

        var request = new Mock<IMemberChapterAdminServiceRequest>();
        request.Setup(x => x.Chapter).Returns(chapter);
        request.Setup(x => x.CurrentMember).Returns(currentMember);
        request.Setup(x => x.Platform).Returns(PlatformType.Default);
        request.Setup(x => x.Securable).Returns(ChapterAdminSecurable.Venues);

        return (new VenueAdminService(unitOfWork), request.Object);
    }

    private static Venue SingleVenue(MockOdkContext context, Chapter chapter)
        => context.Set<Venue>().Single(x => x.ChapterId == chapter.Id);

    private static Venue VenueNamed(MockOdkContext context, string name)
        => context.Set<Venue>().Single(x => x.Name == name);
}

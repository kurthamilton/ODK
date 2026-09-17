using System.Diagnostics.CodeAnalysis;
using ODK.Core;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Venues;
using ODK.Services.Geolocation;
using ODK.Services.Places;
using ODK.Services.Venues.Models;
using ODK.Services.Venues.ViewModels;

namespace ODK.Services.Venues;

public class VenueAdminService : OdkAdminServiceBase, IVenueAdminService
{
    /* A place Google has repositioned by a few metres is the same place more precisely located; one that
       has moved is in a different building. Refinements run to single figures and relocations to hundreds,
       so anywhere in this range separates them - the value is a property of what the two mean, not
       something a deployment should differ on. */
    private const double MovedMetres = 50;

    private readonly ILatLongCalculator _latLongCalculator;
    private readonly IPlacesService _placesService;
    private readonly IUnitOfWork _unitOfWork;

    public VenueAdminService(
        IUnitOfWork unitOfWork,
        IPlacesService placesService,
        ILatLongCalculator latLongCalculator)
        : base(unitOfWork)
    {
        _latLongCalculator = latLongCalculator;
        _placesService = placesService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ArchiveVenue(IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var chapter = request.Chapter;

        var chapterVenue = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterVenueRepository.Query().ForChapter(chapter.Id).ForVenue(venueId).GetSingle());

        if (chapterVenue.ArchivedUtc != null)
        {
            return ServiceResult.Successful();
        }

        chapterVenue.ArchivedUtc = DateTime.UtcNow;
        _unitOfWork.ChapterVenueRepository.Update(chapterVenue);

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> CreateVenue(
        IMemberChapterAdminServiceRequest request, VenueCreateModel model)
    {
        var chapter = request.Chapter;

        if (string.IsNullOrEmpty(model.ExternalId))
        {
            return ServiceResult.Failure("Location required");
        }

        /* Resolved before anything is read or written: the place decides the venue's name, position and
           slug, so there is nothing to compare or create until it answers. */
        var placeResult = await _placesService.GetPlace(model.ExternalId);
        if (!placeResult.Success || placeResult.Place == null)
        {
            return ServiceResult.Failure(placeResult.NotFound
                ? "That location could not be found - search for it again"
                : "Location could not be looked up");
        }

        var place = placeResult.Place;
        var slugBase = SlugBase(place);

        var (latest, chapterVenues, slugCandidates) = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetLatestByExternalLocationId(place.ExternalId),
            x => x.ChapterVenueRepository.Query(q => q.ForChapter(chapter.Id)).ToVenue().GetAll(),
            x => x.VenueRepository.Query(q => q.SlugStartingWith(slugBase)).GetAll());

        /* The same place described the same way is the same venue, however many chapters reach it. A
           different name or a moved position is a new record of it - see IsSameRecord. */
        var venue = IsSameRecord(latest, place)
            ? latest.Venue
            : CreateVenue(place, slugBase, slugCandidates);

        if (chapterVenues.Any(x => x.Id == venue.Id))
        {
            return ServiceResult.Failure("You already have this venue");
        }

        _unitOfWork.ChapterVenueRepository.Add(new ChapterVenue
        {
            AdditionalInfo = model.AdditionalInfo,
            ChapterId = chapter.Id,
            Name = ChapterName(model.Name, place),
            VenueId = venue.Id
        });

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteVenue(IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var chapter = request.Chapter;

        var (chapterVenue, hasEvents) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterVenueRepository.Query(q => q.ForVenue(venueId).ForChapter(chapter.Id)).GetSingle(),
            x => x.EventRepository.Query().ForVenue(venueId).Any());

        // Every chapter's events, not just this one's: the venue itself is about to go.
        if (hasEvents)
        {
            return ServiceResult.Failure("Cannot delete a venue with events");
        }

        // ChapterVenues and VenueLocations both cascade from Venues, so this is the whole delete.
        _unitOfWork.VenueRepository.Delete(chapterVenue.Venue);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<Venue> GetVenue(
        IMemberChapterAdminServiceRequest request, Guid venueId)
        => await GetChapterVenue(request, venueId);

    public async Task<VenueEventsAdminPageViewModel> GetVenueEventsViewModel(
        IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (venue, events) = await GetChapterAdminRestrictedContent(
            request,
            x => ChapterVenueQuery(x, chapter.Id, venueId),
            x => x.EventRepository.GetByVenueId(venueId));

        OdkAssertions.Exists(venue);

        return new VenueEventsAdminPageViewModel
        {
            Chapter = chapter,
            Events = events,
            Platform = platform,
            Venue = venue
        };
    }

    public async Task<VenuesAdminPageViewModel> GetVenuesViewModel(
        IMemberChapterAdminServiceRequest request, bool archived)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (venues, otherVenueCount) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterVenueRepository
                .Query(x => x.ForChapter(chapter.Id).Archived(archived))
                .ToVenue()
                .WithEventSummary()
                .GetAll(),
            x => x.ChapterVenueRepository
                .Query(x => x.ForChapter(chapter.Id).Archived(!archived))
                .Count());

        return new VenuesAdminPageViewModel
        {
            ActiveVenueCount = !archived ? venues.Count : otherVenueCount,
            Archived = archived,
            ArchivedVenueCount = archived ? venues.Count : otherVenueCount,
            Chapter = chapter,
            Venues = venues
        };
    }

    public async Task<VenueAdminPageViewModel> GetVenueViewModel(
        IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (chapterVenue, location) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterVenueRepository
                .Query(q => q.ForChapter(chapter.Id).ForVenue(venueId))
                .GetSingle(),
            x => x.VenueLocationRepository.GetByVenueId(venueId));

        return new VenueAdminPageViewModel
        {
            Chapter = chapter,
            ChapterVenue = chapterVenue,
            Location = location,
            Platform = platform,
            Venue = chapterVenue.Venue
        };
    }

    public async Task<ServiceResult> RestoreVenue(IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var chapter = request.Chapter;

        var chapterVenue = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterVenueRepository.Query().ForChapter(chapter.Id).ForVenue(venueId).GetSingle());

        if (chapterVenue.ArchivedUtc == null)
        {
            return ServiceResult.Successful();
        }

        chapterVenue.ArchivedUtc = null;
        _unitOfWork.ChapterVenueRepository.Update(chapterVenue);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateVenue(
        IMemberChapterAdminServiceRequest request, Guid id, VenueUpdateModel model)
    {
        var chapter = request.Chapter;

        var chapterVenue = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterVenueRepository.Query(x => x.ForChapter(chapter.Id).ForVenue(id)).GetSingle());

        /* Only the link. The venue's name, slug and position are what the lookup returned, and are shared
           with every other chapter using the place. */
        chapterVenue.AdditionalInfo = model.AdditionalInfo;
        chapterVenue.Name = ChapterName(model.Name, chapterVenue.Venue);

        _unitOfWork.ChapterVenueRepository.Update(chapterVenue);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    /// <summary>
    /// A venue of this chapter's, by id. Membership is a row in ChapterVenues rather than a column on
    /// the venue, so the query carries it: a venue no chapter link joins to this one is simply a miss,
    /// which is the 404 that asserting on a loaded venue used to produce.
    /// </summary>
    private static IDeferredQuerySingleOrDefault<Venue> ChapterVenueQuery(
        IUnitOfWork unitOfWork, Guid chapterId, Guid venueId)
        => unitOfWork.ChapterVenueRepository
            .Query(x => x.ForChapter(chapterId).ForVenue(venueId))
            .ToVenue()
            .GetSingleOrDefault();

    /// <summary>
    /// What this chapter calls the venue, or null where it calls it what the place is called. Stored as
    /// null rather than a copy so a chapter that never renamed anything keeps following the place.
    /// </summary>
    private static string? ChapterName(string? submitted, Venue venue)
        => ChapterName(submitted, venue.Name);

    /// <inheritdoc cref="ChapterName(string?, Venue)"/>
    private static string? ChapterName(string? submitted, Place place)
        => ChapterName(submitted, place.Name);

    private static string? ChapterName(string? submitted, string placeName)
    {
        var name = submitted?.NormaliseWhitespace();

        return !string.IsNullOrEmpty(name)
            && !string.Equals(name, placeName, StringComparison.OrdinalIgnoreCase)
            ? name
            : null;
    }

    /// <summary>
    /// A slug unique across the site, from the place's name and the town it is in - "The Oak" in Sheffield
    /// gives <c>the-oak-sheffield</c>. <paramref name="candidates"/> is every venue whose slug starts with
    /// <paramref name="slugBase"/>, which is the whole set a version of it could collide with.
    /// </summary>
    /// <remarks>
    /// Compared case-insensitively to match SQL Server's default collation, so the slugs stay unique under
    /// the unique index on Slug.
    /// </remarks>
    private static string CreateSlug(string slugBase, IReadOnlyCollection<Venue> candidates)
    {
        var taken = candidates
            .Select(x => x.Slug)
            .Where(x => !string.IsNullOrEmpty(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // slugBase is already a slug and never empty, so this always produces one.
        return UrlUtils.SlugifyUnique(slugBase, taken, Venue.SlugMaxLength)!;
    }

    /// <summary>
    /// The slug a place takes before versioning, and so the prefix that finds every slug it could collide
    /// with. A place whose name and town have nothing sluggable between them falls back to a generic slug
    /// that the versioning then keeps unique.
    /// </summary>
    private static string SlugBase(Place place)
    {
        var source = !string.IsNullOrEmpty(place.Locality)
            ? $"{place.Name} {place.Locality}"
            : place.Name;

        return UrlUtils.SlugBase(source, Venue.SlugMaxLength) ?? Venue.SlugFallback;
    }

    private async Task<Venue> GetChapterVenue(
        IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var venue = await GetChapterAdminRestrictedContent(
            request,
            x => ChapterVenueQuery(x, request.Chapter.Id, venueId));

        return OdkAssertions.Exists(venue);
    }

    private Venue CreateVenue(Place place, string slugBase, IReadOnlyCollection<Venue> slugCandidates)
    {
        var venue = _unitOfWork.VenueRepository.Add(new Venue
        {
            CreatedUtc = DateTime.UtcNow,
            MapQuery = place.FormattedAddress,
            Name = place.Name,
            Slug = CreateSlug(slugBase, slugCandidates)
        });

        _unitOfWork.VenueLocationRepository.Add(new VenueLocation
        {
            ExternalId = place.ExternalId,
            Latitude = place.Location.Lat,
            Longitude = place.Location.Long,
            MapQuery = place.FormattedAddress,
            Name = place.FormattedAddress ?? place.Name,
            VenueId = venue.Id
        });

        return venue;
    }

    /// <summary>
    /// Whether the latest record of this place still describes it. The name has to match exactly; the
    /// position only has to be within <see cref="MovedMetres"/>, because a lookup refining where a place
    /// sits is not the place moving.
    /// </summary>
    private bool IsSameRecord([NotNullWhen(true)] VenueWithLocationDto? latest, Place place)
    {
        if (latest?.Location == null)
        {
            return false;
        }

        if (!string.Equals(latest.Venue.Name, place.Name, StringComparison.Ordinal))
        {
            return false;
        }

        var metres = _latLongCalculator.CalculateMetresBetween(latest.Location.LatLong, place.Location);

        return metres <= MovedMetres;
    }
}

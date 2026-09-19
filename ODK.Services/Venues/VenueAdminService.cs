using System.Diagnostics.CodeAnalysis;
using ODK.Core;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Data.Core;
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
    private readonly IVenueSlugService _venueSlugService;

    public VenueAdminService(
        IUnitOfWork unitOfWork,
        IPlacesService placesService,
        ILatLongCalculator latLongCalculator,
        IVenueSlugService venueSlugService)
        : base(unitOfWork)
    {
        _latLongCalculator = latLongCalculator;
        _placesService = placesService;
        _unitOfWork = unitOfWork;
        _venueSlugService = venueSlugService;
    }

    public async Task<ServiceResult> ArchiveVenue(IMemberChapterAdminServiceRequest request, Guid chapterVenueId)
    {
        var chapter = request.Chapter;

        var chapterVenue = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterVenueRepository.GetById(chapterVenueId));

        OdkAssertions.BelongsToChapter(chapterVenue, chapter.Id);

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
        var slugBase = _venueSlugService.SlugBase(place);

        var (latestVenue, slugCandidates) = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetLatestByExternalLocationId(place.ExternalId),
            x => x.VenueRepository.Query(q => q.SlugStartingWith(slugBase)).GetAll());

        /* The same place described the same way is the same venue, however many chapters reach it. A
           different name or a moved position is a new record of it - see IsSameRecord. */
        Venue venue;

        if (IsSameRecord(latestVenue, place))
        {
            venue = latestVenue.Venue;

            var chapterVenueExists = await _unitOfWork.ChapterVenueRepository.Query()
                .ForVenue(venue.Id)
                .ForChapter(chapter.Id)
                .Any()
                .Run();

            if (chapterVenueExists)
            {
                return ServiceResult.Failure("You already have this venue");
            }
        }
        else
        {
            venue = CreateVenue(place, slugBase, slugCandidates);
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

    public async Task<ChapterVenue> GetChapterVenue(IMemberChapterAdminServiceRequest request, Guid chapterVenueId)
    {
        var chapter = request.Chapter;

        var chapterVenue = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterVenueRepository.GetById(chapterVenueId));

        return OdkAssertions.BelongsToChapter(chapterVenue, chapter.Id);
    }

    public async Task<VenueCreateAdminPageViewModel> GetVenueCreateViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var chapterLocation = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterLocationRepository.GetByChapterId(request.Chapter.Id));

        return new VenueCreateAdminPageViewModel
        {
            ChapterLocation = chapterLocation
        };
    }

    public async Task<VenueEventsAdminPageViewModel> GetVenueEventsViewModel(
        IMemberChapterAdminServiceRequest request, Guid chapterVenueId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (chapterVenue, events) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterVenueRepository.GetById(chapterVenueId),
            x => x.EventRepository.Query().ForChapterVenue(chapterVenueId).GetAll());

        OdkAssertions.BelongsToChapter(chapterVenue, chapter.Id);

        return new VenueEventsAdminPageViewModel
        {
            Chapter = chapter,
            ChapterVenue = chapterVenue,
            Events = events,
            Platform = platform
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
        IMemberChapterAdminServiceRequest request, Guid chapterVenueId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (dto, chapterLocation) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterVenueRepository.Query()
                .ById(chapterVenueId)
                .WithLocation()
                .GetSingle(),
            x => x.ChapterLocationRepository.GetByChapterId(chapter.Id));

        OdkAssertions.BelongsToChapter(dto.ChapterVenue, chapter.Id);

        return new VenueAdminPageViewModel
        {
            Chapter = chapter,
            ChapterLocation = chapterLocation,
            ChapterVenue = dto.ChapterVenue,
            Location = dto.Location,
            Platform = platform
        };
    }

    public async Task<ServiceResult> RemoveVenue(IMemberChapterAdminServiceRequest request, Guid chapterVenueId)
    {
        var chapter = request.Chapter;

        var (chapterVenue, hasEvents) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterVenueRepository.GetById(chapterVenueId),
            x => x.EventRepository.Query().ForChapterVenue(chapterVenueId).Any());

        OdkAssertions.BelongsToChapter(chapterVenue, chapter.Id);

        // This chapter's events, because the venue is staying: another chapter's are not its business.
        if (hasEvents)
        {
            return ServiceResult.Failure("Cannot remove a venue with events");
        }

        /* Only the link. The venue is site-level and other chapters may be using it, so one of them
           tidying up cannot take it away from the rest. A venue nothing links to any more is an orphan,
           and deleting those is the site admin's. */
        _unitOfWork.ChapterVenueRepository.Delete(chapterVenue);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> RestoreVenue(IMemberChapterAdminServiceRequest request, Guid chapterVenueId)
    {
        var chapter = request.Chapter;

        var chapterVenue = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterVenueRepository.GetById(chapterVenueId));

        OdkAssertions.BelongsToChapter(chapterVenue, chapter.Id);

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
        IMemberChapterAdminServiceRequest request, Guid chapterVenueId, VenueUpdateModel model)
    {
        var chapter = request.Chapter;

        var chapterVenue = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterVenueRepository.GetById(chapterVenueId));

        OdkAssertions.BelongsToChapter(chapterVenue, chapter.Id);

        /* Only the link. The venue's name, slug and position are what the lookup returned, and are shared
           with every other chapter using the place. */
        chapterVenue.AdditionalInfo = model.AdditionalInfo;
        chapterVenue.Name = ChapterName(model.Name, chapterVenue.Venue);

        _unitOfWork.ChapterVenueRepository.Update(chapterVenue);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

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

    private Venue CreateVenue(Place place, string slugBase, IReadOnlyCollection<Venue> slugCandidates)
    {
        var venue = _unitOfWork.VenueRepository.Add(new Venue
        {
            CreatedUtc = DateTime.UtcNow,
            Name = place.Name,
            Slug = _venueSlugService.CreateSlug(slugBase, slugCandidates)
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

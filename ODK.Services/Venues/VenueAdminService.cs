using ODK.Core;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Services.Venues.Models;
using ODK.Services.Venues.ViewModels;

namespace ODK.Services.Venues;

public class VenueAdminService : OdkAdminServiceBase, IVenueAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public VenueAdminService(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ArchiveVenue(IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var chapter = request.Chapter;

        var venue = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetById(venueId));

        OdkAssertions.BelongsToChapter(venue, chapter.Id);

        if (venue.ArchivedUtc != null)
        {
            return ServiceResult.Successful();
        }

        venue.ArchivedUtc = DateTime.UtcNow;
        _unitOfWork.VenueRepository.Update(venue);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> CreateVenue(
        IMemberChapterAdminServiceRequest request, VenueCreateModel model)
    {
        var chapter = request.Chapter;

        // Normalised before the duplicate lookup, not after: whitespace must not let " Oak" or
        // "The  Oak" through as a second venue alongside "Oak" / "The Oak". Both would pass the unique
        // index on (ChapterId, Name) as distinct names, then collide on slug.
        var name = model.Name.NormaliseWhitespace();

        var chapterVenues = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetByChapterId(chapter.Id));

        var existing = FindByName(chapterVenues, name);

        var venue = new Venue
        {
            Address = model.Address,
            ChapterId = chapter.Id,
            MapQuery = model.LocationName,
            Name = name
        };

        var location = new VenueLocation
        {
            Latitude = model.Location?.Lat ?? 0,
            Longitude = model.Location?.Long ?? 0,
            Name = model.LocationName ?? string.Empty
        };

        var validationResult = ValidateVenue(venue, existing, location);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        venue.Slug = CreateSlug(venue.Name, chapterVenues, venueId: null);

        _unitOfWork.VenueRepository.Add(venue);

        location.VenueId = venue.Id;
        _unitOfWork.VenueLocationRepository.Add(location);

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteVenue(IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var chapter = request.Chapter;

        var (venue, hasEvents) = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetById(venueId),
            x => x.EventRepository.Query().ForVenue(venueId).Any());

        OdkAssertions.BelongsToChapter(venue, chapter.Id);

        if (hasEvents)
        {
            return ServiceResult.Failure("Cannot delete a venue with events");
        }

        _unitOfWork.VenueRepository.Delete(venue);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<Venue> GetVenue(
        IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var chapter = request.Chapter;

        var venue = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetById(venueId));

        return OdkAssertions.BelongsToChapter(venue, chapter.Id);
    }

    public async Task<VenueEventsAdminPageViewModel> GetVenueEventsViewModel(
        IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (venue, events) = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetById(venueId),
            x => x.EventRepository.GetByVenueId(venueId));

        OdkAssertions.BelongsToChapter(venue, chapter.Id);

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
            x => x.VenueRepository
                .Query(x => x.ForChapter(chapter.Id).Archived(archived))
                .WithEventSummary()
                .GetAll(),
            x => x.VenueRepository
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

        var (venue, location) = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetById(venueId),
            x => x.VenueLocationRepository.GetByVenueId(venueId));

        OdkAssertions.BelongsToChapter(venue, chapter.Id);

        return new VenueAdminPageViewModel
        {
            Chapter = chapter,
            Location = location,
            Platform = platform,
            Venue = venue
        };
    }

    public async Task<ServiceResult> RestoreVenue(IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var chapter = request.Chapter;

        var venue = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetById(venueId));

        OdkAssertions.BelongsToChapter(venue, chapter.Id);

        if (venue.ArchivedUtc == null)
        {
            return ServiceResult.Successful();
        }

        venue.ArchivedUtc = null;
        _unitOfWork.VenueRepository.Update(venue);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateVenue(
        IMemberChapterAdminServiceRequest request, Guid id, VenueCreateModel model)
    {
        var chapter = request.Chapter;

        // Normalised before the duplicate lookup - see CreateVenue.
        var name = model.Name.NormaliseWhitespace();

        var (location, chapterVenues) = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueLocationRepository.GetByVenueId(id),
            x => x.VenueRepository.GetByChapterId(chapter.Id));

        // chapterVenues is already scoped to the chapter, so a miss covers both "no such venue" and
        // "not this chapter's venue"; BelongsToChapter asserts existence first, so either is a 404.
        var venue = OdkAssertions.BelongsToChapter(
            chapterVenues.FirstOrDefault(x => x.Id == id), chapter.Id);

        var existing = FindByName(chapterVenues, name);

        venue.Address = model.Address;
        venue.MapQuery = model.LocationName;
        venue.Name = name;

        location ??= new VenueLocation();

        location.Name = model.LocationName ?? string.Empty;
        location.Latitude = model.Location?.Lat ?? 0;
        location.Longitude = model.Location?.Long ?? 0;

        var validationResult = ValidateVenue(venue, existing, location);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        venue.Slug = CreateSlug(venue.Name, chapterVenues, venue.Id);

        _unitOfWork.VenueRepository.Update(venue);

        if (location.VenueId == default)
        {
            location.VenueId = venue.Id;
            _unitOfWork.VenueLocationRepository.Add(location);
        }
        else
        {
            _unitOfWork.VenueLocationRepository.Update(location);
        }

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    /// <summary>
    /// A slug unique within the chapter. <paramref name="venueId"/> is excluded from the taken set so
    /// that renaming a venue to another form of its own name (e.g. "The Oak" to "The Oak!") keeps its
    /// slug rather than colliding with itself and versioning to "the-oak-2".
    /// </summary>
    /// <remarks>
    /// Compared case-insensitively to match SQL Server's default collation, so the slugs stay unique
    /// under the unique index this is building towards. Archived venues keep their slugs and are
    /// counted, so restoring one can never introduce a duplicate.
    /// </remarks>
    private static string CreateSlug(string name, IReadOnlyCollection<Venue> chapterVenues, Guid? venueId)
    {
        var taken = chapterVenues
            .Where(x => x.Id != venueId)
            .Select(x => x.Slug)
            .Where(x => !string.IsNullOrEmpty(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // A name with no letters or digits at all slugs to nothing, and the column is required, so it
        // falls back to a generic slug that the versioning then keeps unique.
        return UrlUtils.SlugifyUnique(name, taken, Venue.SlugMaxLength)
            ?? UrlUtils.SlugifyUnique(Venue.SlugFallback, taken, Venue.SlugMaxLength)!;
    }

    /// <summary>
    /// The chapter's venue of that name, if any. Both stored and candidate names are normalised before
    /// comparing, so a legacy name saved before normalisation ("The  Oak") is still recognised as the
    /// same venue as "The Oak" — the database's unique index would reject the insert anyway, and a
    /// friendly failure beats an unhandled constraint violation.
    /// </summary>
    private static Venue? FindByName(IReadOnlyCollection<Venue> chapterVenues, string name)
        => chapterVenues.FirstOrDefault(
            x => string.Equals(x.Name.NormaliseWhitespace(), name, StringComparison.OrdinalIgnoreCase));

    private ServiceResult ValidateVenue(Venue venue, Venue? existing, VenueLocation location)
    {
        if (string.IsNullOrWhiteSpace(venue.Name))
        {
            return ServiceResult.Failure("Name required");
        }

        if (existing != null && existing.Id != venue.Id)
        {
            return ServiceResult.Failure("Venue with that name already exists");
        }

        if (string.IsNullOrEmpty(location.Name) || location.LatLong.IsDefault)
        {
            return ServiceResult.Failure("Location not set");
        }

        return ServiceResult.Successful();
    }
}
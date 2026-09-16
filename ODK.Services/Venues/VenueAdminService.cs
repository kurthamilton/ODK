using ODK.Core;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
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

        // Normalised before the duplicate lookup, not after: whitespace must not let " Oak" or
        // "The  Oak" through as a second venue alongside "Oak" / "The Oak". Nothing in the database
        // stops it - a name is unique to a chapter only by this check - and the two would then collide
        // on slug.
        var name = model.Name.NormaliseWhitespace();
        var slugBase = SlugBase(name);

        var (chapterVenues, slugCandidates) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterVenueRepository.Query(x => x.ForChapter(chapter.Id)).ToVenue().GetAll(),
            x => x.VenueRepository.Query(q => q.SlugStartingWith(slugBase)).GetAll());

        var existing = FindByName(chapterVenues, name);

        var venue = _unitOfWork.VenueRepository.Add(new Venue
        {
            Address = model.Address,
            MapQuery = model.LocationName,
            Name = name,
            Slug = CreateSlug(slugBase, slugCandidates, venueId: null)
        });

        var location = _unitOfWork.VenueLocationRepository.Add(new VenueLocation
        {
            Latitude = model.Location?.Lat ?? 0,
            Longitude = model.Location?.Long ?? 0,
            Name = model.LocationName ?? string.Empty,
            VenueId = venue.Id
        });

        var validationResult = ValidateVenue(venue, existing, location);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterVenueRepository.Add(new ChapterVenue
        {
            ChapterId = chapter.Id,
            VenueId = venue.Id,
            Venue = venue
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

        var (venue, location) = await GetChapterAdminRestrictedContent(
            request,
            x => ChapterVenueQuery(x, chapter.Id, venueId),
            x => x.VenueLocationRepository.GetByVenueId(venueId));

        OdkAssertions.Exists(venue);

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
        IMemberChapterAdminServiceRequest request, Guid id, VenueCreateModel model)
    {
        var chapter = request.Chapter;

        // Normalised before the duplicate lookup - see CreateVenue.
        var name = model.Name.NormaliseWhitespace();
        var slugBase = SlugBase(name);

        var (location, chapterVenues, slugCandidates) = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueLocationRepository.GetByVenueId(id),
            x => x.ChapterVenueRepository.Query(x => x.ForChapter(chapter.Id)).ToVenue().GetAll(),
            x => x.VenueRepository.Query(q => q.SlugStartingWith(slugBase)).GetAll());

        // chapterVenues holds the venues linked to this chapter, so a miss covers both "no such venue"
        // and "not one of this chapter's venues"; either is a 404.
        var venue = OdkAssertions.Exists(chapterVenues.FirstOrDefault(x => x.Id == id));

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

        venue.Slug = CreateSlug(slugBase, slugCandidates, venue.Id);

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
    /// A slug unique across the site. <paramref name="candidates"/> is every venue whose slug starts
    /// with <paramref name="slugBase"/>, which is the whole set a version of it could collide with.
    /// <paramref name="venueId"/> is excluded from it so that renaming a venue to another form of its
    /// own name (e.g. "The Oak" to "The Oak!") keeps its slug rather than colliding with itself and
    /// versioning to "the-oak-2".
    /// </summary>
    /// <remarks>
    /// Compared case-insensitively to match SQL Server's default collation, so the slugs stay unique
    /// under the unique index this is building towards. Archived venues keep their slugs and are
    /// counted, so restoring one can never introduce a duplicate.
    /// </remarks>
    private static string CreateSlug(
        string slugBase, IReadOnlyCollection<Venue> candidates, Guid? venueId)
    {
        var taken = candidates
            .Where(x => x.Id != venueId)
            .Select(x => x.Slug)
            .Where(x => !string.IsNullOrEmpty(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // slugBase is already a slug and never empty, so this always produces one.
        return UrlUtils.SlugifyUnique(slugBase, taken, Venue.SlugMaxLength)!;
    }

    /// <summary>
    /// The chapter's venue of that name, if any. Both stored and candidate names are normalised before
    /// comparing, so a legacy name saved before normalisation ("The  Oak") is still recognised as the
    /// same venue as "The Oak". This is the only thing keeping a chapter's venue names distinct; the
    /// database holds no constraint on them, since a name belongs to the site rather than to a chapter.
    /// </summary>
    private static Venue? FindByName(IReadOnlyCollection<Venue> chapterVenues, string name)
        => chapterVenues.FirstOrDefault(
            x => string.Equals(x.Name.NormaliseWhitespace(), name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The slug a venue of that name takes before versioning, and so the prefix that finds every slug
    /// it could collide with. A name with no letters or digits at all slugs to nothing, and the column
    /// is required, so it falls back to a generic slug that the versioning then keeps unique.
    /// </summary>
    private static string SlugBase(string name)
        => UrlUtils.SlugBase(name, Venue.SlugMaxLength) ?? Venue.SlugFallback;

    private async Task<Venue> GetChapterVenue(
        IMemberChapterAdminServiceRequest request, Guid venueId)
    {
        var venue = await GetChapterAdminRestrictedContent(
            request,
            x => ChapterVenueQuery(x, request.Chapter.Id, venueId));

        return OdkAssertions.Exists(venue);
    }

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
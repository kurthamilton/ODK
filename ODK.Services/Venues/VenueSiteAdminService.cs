using ODK.Core;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Services.Places;
using ODK.Services.Venues.ViewModels;

namespace ODK.Services.Venues;

public class VenueSiteAdminService : OdkAdminServiceBase, IVenueSiteAdminService
{
    private readonly IPlacesService _placesService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVenueSlugService _venueSlugService;

    public VenueSiteAdminService(
        IUnitOfWork unitOfWork,
        IPlacesService placesService,
        IVenueSlugService venueSlugService)
        : base(unitOfWork)
    {
        _placesService = placesService;
        _unitOfWork = unitOfWork;
        _venueSlugService = venueSlugService;
    }

    /// <summary>
    /// Resolves a venue to a place and sets its name, slug and position from it - the details a venue
    /// created today takes from the lookup, applied to one that predates it.
    /// </summary>
    /// <remarks>
    /// The venue is changed in place rather than superseded by a new one. A new record is what a place
    /// <em>changing</em> earns, and nothing has changed here: the place was always this one, and the venue
    /// was only ever describing it in the words somebody typed. Superseding it would also strand the
    /// events and links that already point at it.
    /// </remarks>
    public async Task<ServiceResult> BackfillVenue(
        IMemberServiceRequest request, Guid venueId, string? externalId)
    {
        if (string.IsNullOrEmpty(externalId))
        {
            return ServiceResult.Failure("Location required");
        }

        var placeResult = await _placesService.GetPlace(externalId);
        if (!placeResult.Success || placeResult.Place == null)
        {
            return ServiceResult.Failure(placeResult.NotFound
                ? "That location could not be found - search for it again"
                : "Location could not be looked up");
        }

        var place = placeResult.Place;
        var slugBase = _venueSlugService.SlugBase(place);

        var (venue, location, chapterVenues, slugCandidates, recordOfPlace) =
            await GetSiteAdminRestrictedContent(
                request,
                x => x.VenueRepository.GetByIdOrDefault(venueId),
                x => x.VenueLocationRepository.GetByVenueId(venueId),
                x => x.ChapterVenueRepository.Query(q => q.ForVenue(venueId)).GetAll(),
                x => x.VenueRepository.Query(q => q.SlugStartingWith(slugBase)).GetAll(),
                x => x.VenueRepository.GetLatestByExternalLocationId(place.ExternalId));

        OdkAssertions.Exists(venue);

        /* One venue per place, which is what lets two groups adding the same place share a record. Merging
           this venue into the one already holding the place would have to move its events and its links,
           and decide which name survives - so the two are left for a site admin to resolve by hand. */
        if (recordOfPlace != null && recordOfPlace.Venue.Id != venue.Id)
        {
            return ServiceResult.Failure(
                $"'{recordOfPlace.Venue.Name}' is already this place");
        }

        /* Before the venue takes the place's name, because what it is called now is what an organiser
           typed - a group's own name for the place, which is exactly what the link is for. Only where the
           two differ, and only where a group has not already named it something else. */
        foreach (var chapterVenue in chapterVenues)
        {
            if (chapterVenue.Name == null &&
                !string.Equals(venue.Name, place.Name, StringComparison.OrdinalIgnoreCase))
            {
                chapterVenue.Name = venue.Name;
                _unitOfWork.ChapterVenueRepository.Update(chapterVenue);
            }
        }

        venue.Name = place.Name;

        /* Its own slug is in the candidate set, so a venue already sitting on this place's slug would
           otherwise be versioned away from it. */
        venue.Slug = _venueSlugService.CreateSlug(
            slugBase, slugCandidates.Where(x => x.Id != venue.Id).ToArray());

        _unitOfWork.VenueRepository.Update(venue);

        if (location != null)
        {
            location.ExternalId = place.ExternalId;
            location.Latitude = place.Location.Lat;
            location.Longitude = place.Location.Long;
            location.MapQuery = place.FormattedAddress;
            location.Name = place.FormattedAddress ?? place.Name;
            _unitOfWork.VenueLocationRepository.Update(location);
        }
        else
        {
            // A venue recorded before locations were, which has nothing to update.
            _unitOfWork.VenueLocationRepository.Add(new VenueLocation
            {
                ExternalId = place.ExternalId,
                Latitude = place.Location.Lat,
                Longitude = place.Location.Long,
                MapQuery = place.FormattedAddress,
                Name = place.FormattedAddress ?? place.Name,
                VenueId = venue.Id
            });
        }

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    /// <summary>
    /// Deletes a venue no chapter links to. Orphans arise in the ordinary course of things - a chapter
    /// removing its last link, a chapter being deleted, a record of a place superseded by a later one -
    /// so this is tidying rather than a correction.
    /// </summary>
    public async Task<ServiceResult> DeleteVenue(IMemberServiceRequest request, Guid venueId)
    {
        var (venue, chapterCount, hasEvents) = await GetSiteAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetByIdOrDefault(venueId),
            x => x.ChapterVenueRepository.Query(q => q.ForVenue(venueId)).Count(),
            x => x.EventRepository.Query().ForVenue(venueId).Any());

        OdkAssertions.Exists(venue);

        if (chapterCount > 0)
        {
            return ServiceResult.Failure("Cannot delete a venue a group is using");
        }

        /* An event outlives the link that made it: a chapter can remove a venue it no longer uses while
           its past events still point at it. An event's venue is required and its foreign key restricts,
           so this is what the database would refuse anyway - said here, where it can be read. */
        if (hasEvents)
        {
            return ServiceResult.Failure("Cannot delete a venue with events");
        }

        _unitOfWork.VenueRepository.Delete(venue);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<VenueSiteAdminPageViewModel> GetVenueViewModel(
        IMemberServiceRequest request, Guid venueId)
    {
        var (venue, location, chapterCount) = await GetSiteAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetByIdOrDefault(venueId),
            x => x.VenueLocationRepository.GetByVenueId(venueId),
            x => x.ChapterVenueRepository.Query(q => q.ForVenue(venueId)).Count());

        return new VenueSiteAdminPageViewModel
        {
            ChapterCount = chapterCount,
            Location = location,
            Venue = OdkAssertions.Exists(venue)
        };
    }

    public async Task<VenuesSiteAdminPageViewModel> GetVenuesViewModel(IMemberServiceRequest request)
    {
        var venues = await GetSiteAdminRestrictedContent(
            request,
            x => x.VenueRepository.Query().WithChapterCount().GetAll());

        return new VenuesSiteAdminPageViewModel
        {
            Venues = venues
        };
    }
}

using ODK.Core.Venues;
using ODK.Services.Venues.Models;
using ODK.Services.Venues.ViewModels;

namespace ODK.Services.Venues;

public interface IVenueAdminService
{
    Task<ServiceResult> ArchiveVenue(IMemberChapterAdminServiceRequest request, Guid venueId);

    /// <summary>
    /// Temporary, for the Venue.Slug rollout — remove once the column is required.
    /// </summary>
    Task<ServiceResult> BackfillSlugs(IMemberServiceRequest request);

    Task<ServiceResult> CreateVenue(IMemberChapterAdminServiceRequest request, VenueCreateModel venue);

    Task<Venue> GetVenue(IMemberChapterAdminServiceRequest request, Guid venueId);

    Task<VenueEventsAdminPageViewModel> GetVenueEventsViewModel(IMemberChapterAdminServiceRequest request, Guid venueId);

    Task<VenuesAdminPageViewModel> GetVenuesViewModel(IMemberChapterAdminServiceRequest request, bool archived);

    Task<VenueAdminPageViewModel> GetVenueViewModel(IMemberChapterAdminServiceRequest request, Guid venueId);

    Task<ServiceResult> RestoreVenue(IMemberChapterAdminServiceRequest request, Guid venueId);

    Task<ServiceResult> UpdateVenue(IMemberChapterAdminServiceRequest request, Guid id, VenueCreateModel venue);
}
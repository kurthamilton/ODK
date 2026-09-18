using ODK.Services.Venues.ViewModels;

namespace ODK.Services.Venues;

public interface IVenueSiteAdminService
{
    /// <summary>
    /// Resolves a venue to a place and sets its details from it, applying the rules a venue created today
    /// would follow. For venues that predate the lookup, which carry whatever an organiser typed.
    /// </summary>
    Task<ServiceResult> BackfillVenue(IMemberServiceRequest request, Guid venueId, string? externalId);

    Task<ServiceResult> DeleteVenue(IMemberServiceRequest request, Guid venueId);

    Task<VenueSiteAdminPageViewModel> GetVenueViewModel(IMemberServiceRequest request, Guid venueId);

    Task<VenuesSiteAdminPageViewModel> GetVenuesViewModel(IMemberServiceRequest request);
}

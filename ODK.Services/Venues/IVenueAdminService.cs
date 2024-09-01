using ODK.Core.Venues;
using ODK.Services.Venues.ViewModels;

namespace ODK.Services.Venues;

public interface IVenueAdminService
{
    Task<ServiceResult> CreateVenue(AdminServiceRequest request, CreateVenue venue);

    Task<Venue> GetVenue(AdminServiceRequest request, Guid venueId);

    Task<VenueEventsAdminPageViewModel> GetVenueEventsViewModel(AdminServiceRequest request, Guid venueId);

    Task<VenueLocation?> GetVenueLocation(AdminServiceRequest request, Venue venue);

    Task<IReadOnlyCollection<Venue>> GetVenues(AdminServiceRequest request);

    Task<VenuesAdminPageViewModel> GetVenuesViewModel(AdminServiceRequest request);

    Task<VenueAdminPageViewModel> GetVenueViewModel(AdminServiceRequest request, Guid venueId);

    Task<ServiceResult> UpdateVenue(AdminServiceRequest request, Guid id, CreateVenue venue);
}

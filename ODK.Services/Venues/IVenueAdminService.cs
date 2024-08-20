using ODK.Core.Venues;

namespace ODK.Services.Venues;

public interface IVenueAdminService
{
    Task<ServiceResult> CreateVenue(AdminServiceRequest request, CreateVenue venue);

    Task<Venue> GetVenue(AdminServiceRequest request, Guid venueId);

    Task<VenueLocation?> GetVenueLocation(AdminServiceRequest request, Venue venue);

    Task<IReadOnlyCollection<Venue>> GetVenues(AdminServiceRequest request);

    Task<VenuesDto> GetVenuesDto(AdminServiceRequest request);

    Task<ServiceResult> UpdateVenue(AdminServiceRequest request, Guid id, CreateVenue venue);
}

using ODK.Core.Venues;
using ODK.Services.Venues.Models;
using ODK.Services.Venues.ViewModels;

namespace ODK.Services.Venues;

public interface IVenueAdminService
{
    Task<ServiceResult> CreateVenue(MemberChapterAdminServiceRequest request, VenueCreateModel venue);

    Task<Venue> GetVenue(MemberChapterAdminServiceRequest request, Guid venueId);

    Task<VenueEventsAdminPageViewModel> GetVenueEventsViewModel(MemberChapterAdminServiceRequest request, Guid venueId);

    Task<VenuesAdminPageViewModel> GetVenuesViewModel(MemberChapterAdminServiceRequest request);

    Task<VenueAdminPageViewModel> GetVenueViewModel(MemberChapterAdminServiceRequest request, Guid venueId);

    Task<ServiceResult> UpdateVenue(MemberChapterAdminServiceRequest request, Guid id, VenueCreateModel venue);
}

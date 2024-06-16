using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Venues;

namespace ODK.Services.Venues;

public interface IVenueService
{
    Task<Venue> GetVenueAsync(Member? currentMember, Event @event);
}

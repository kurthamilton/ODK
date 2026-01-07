using ODK.Core;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Venues;
using ODK.Data.Core;

namespace ODK.Services.Venues;

public class VenueService : IVenueService
{
    private readonly IUnitOfWork _unitOfWork;

    public VenueService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Venue> GetVenueAsync(Member? currentMember, Event @event)
    {
        var venue = await _unitOfWork.VenueRepository.GetByIdOrDefault(@event.VenueId).Run();
        return OdkAssertions.MeetsCondition(venue,
            _ => @event.IsAuthorized(currentMember));
    }
}

using ODK.Core.Events;
using ODK.Core.Exceptions;
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
        var venue = await _unitOfWork.VenueRepository.GetByIdOrDefault(@event.VenueId).RunAsync();
        if (currentMember == null || venue?.ChapterId != currentMember.ChapterId)
        {
            venue = await _unitOfWork.VenueRepository.GetPublicVenue(@event.Id).RunAsync();
        }

        if (venue == null)
        {
            throw new OdkNotFoundException();
        }

        return venue;
    }
}

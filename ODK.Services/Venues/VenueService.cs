using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Venues;

namespace ODK.Services.Venues;

public class VenueService : IVenueService
{
    private readonly IVenueRepository _venueRepository;

    public VenueService(IVenueRepository venueRepository)
    {
        _venueRepository = venueRepository;
    }
    
    public async Task<Venue> GetVenueAsync(Member? currentMember, Event @event)
    {
        var venue = await _venueRepository.GetVenueAsync(@event.VenueId);
        if (currentMember == null || venue?.ChapterId != currentMember.ChapterId)
        {
            venue = await _venueRepository.GetPublicVenueAsync(@event.VenueId);
        }

        if (venue == null)
        {
            throw new Exception($"Venue not found for event {@event.Id}");
        }

        return venue;
    }
}

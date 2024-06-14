namespace ODK.Core.Venues;

public interface IVenueRepository
{
    Task<Guid> CreateVenue(Venue venue);

    Task<Venue> GetPublicVenue(Guid id);

    Task<Venue?> GetVenue(Guid id);

    Task<Venue?> GetVenueByName(string name);

    Task<IReadOnlyCollection<Venue>> GetVenues(Guid chapterId);

    Task<IReadOnlyCollection<Venue>> GetVenues(Guid chapterId, IEnumerable<Guid> venueIds);

    Task UpdateVenue(Venue venue);
}

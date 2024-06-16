namespace ODK.Core.Venues;

public interface IVenueRepository
{
    Task<Guid> CreateVenueAsync(Venue venue);

    Task<Venue?> GetPublicVenueAsync(Guid id);

    Task<Venue?> GetVenueAsync(Guid id);

    Task<Venue?> GetVenueByNameAsync(string name);

    Task<IReadOnlyCollection<Venue>> GetVenuesAsync(Guid chapterId);

    Task<IReadOnlyCollection<Venue>> GetVenuesAsync(Guid chapterId, IEnumerable<Guid> venueIds);

    Task UpdateVenueAsync(Venue venue);
}

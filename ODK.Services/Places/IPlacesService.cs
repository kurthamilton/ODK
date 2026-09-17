namespace ODK.Services.Places;

public interface IPlacesService
{
    /// <summary>
    /// Looks a place up by the id that identifies it in the place service. The answer is what that
    /// service holds now, so a caller that stores it is recording a moment rather than caching a value.
    /// </summary>
    Task<GetPlaceResult> GetPlace(string externalId);
}

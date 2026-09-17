namespace ODK.Services.Places;

/// <summary>
/// The outcome of <see cref="IPlacesService.GetPlace"/>. A place that no longer exists is its own case
/// rather than a failure message, because the two call for different things: a stale ID is something the
/// member can fix by picking the place again, and anything else is ours to deal with.
/// </summary>
public class GetPlaceResult : ServiceResult
{
    private GetPlaceResult(bool success, string? message, Place? place, bool notFound)
        : base(success, message)
    {
        NotFound = notFound;
        Place = place;
    }

    /// <summary>Google no longer knows this ID.</summary>
    public bool NotFound { get; }

    public Place? Place { get; }

    public new static GetPlaceResult Failure(string message)
        => new GetPlaceResult(false, message, place: null, notFound: false);

    public static GetPlaceResult Found(Place place)
        => new GetPlaceResult(true, message: null, place, notFound: false);

    public static GetPlaceResult PlaceNotFound()
        => new GetPlaceResult(false, "Place not found", place: null, notFound: true);
}

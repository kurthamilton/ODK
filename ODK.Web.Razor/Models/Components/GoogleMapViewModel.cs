using ODK.Core.Countries;

namespace ODK.Web.Razor.Models.Components;

public class GoogleMapViewModel
{
    public const int ZoomLevelCity = 12;

    public string? ExternalId { get; init; }

    public LatLong? LatLong { get; init; }

    public string? Query { get; init; }

    public string? QuerySource { get; init; }

    public bool ShowLink { get; init; }

    public int? Zoom { get; init; }

    public string? GoogleMapsQuery() =>
        !string.IsNullOrEmpty(ExternalId)
            ? $"place_id:{ExternalId}"
            : !string.IsNullOrEmpty(Query)
                ? Query
                : LatLong != null
                    ? $"{LatLong.Value.Lat},{LatLong.Value.Long}"
                    : null;
}

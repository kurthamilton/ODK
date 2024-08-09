using ODK.Core.Countries;

namespace ODK.Web.Razor.Models.Components;

public class GoogleMapViewModel
{
    public const int ZoomLevelCity = 12;

    public LatLong? LatLong { get; init; }

    public string? Query { get; init; }

    public string? QuerySource { get; init; }

    public int? Zoom {  get; init; }
}

using ODK.Core.Countries;

namespace ODK.Web.Razor.Models.Components;

public class GoogleMapViewModel
{
    public LatLong? LatLong { get; init; }

    public string? Query { get; init; }

    public string? QuerySource { get; init; }
}

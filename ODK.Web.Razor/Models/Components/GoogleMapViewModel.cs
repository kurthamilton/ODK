namespace ODK.Web.Razor.Models.Components;

public class GoogleMapViewModel
{
    public required string GoogleMapsApiKey { get; set; }

    public string? Query { get; set; }

    public string? QuerySource { get; set; }
}

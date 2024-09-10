using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages.Groups;

public class IndexModel : OdkPageModel
{
    public double? Distance { get; private set; }

    public string? DistanceUnit { get; private set; }

    public double? Lat {  get; private set; }

    public double? Long { get; private set; }

    public string? LocationName { get; private set; }        

    public string? TopicGroup { get; private set; }

    public void OnGet(
        [FromQuery(Name = "l")] string? latLong,
        [FromQuery(Name = "n")] string? name,
        [FromQuery(Name = "d")] double? distance,
        [FromQuery(Name = "u")] string? unit,
        [FromQuery(Name = "c")] string? topicGroup)
    {
        TopicGroup = topicGroup;
        UpdateDistance(distance, unit);
        UpdateLocation(latLong, name);
    }

    private void UpdateDistance(double? distance, string? unit)
    {
        if (distance == null || unit == null)
        {
            return;
        }

        Distance = distance;
        DistanceUnit = unit;
    }

    private void UpdateLocation(string? latLong, string? name)
    {
        if (latLong == null || name == null)
        {
            return;
        }

        var latLongParts = latLong.Split(',');
        if (latLongParts.Length != 2)
        {
            return;
        }

        if (!double.TryParse(latLongParts[0], out var lat) ||
            !double.TryParse(latLongParts[1], out var @long))
        {
            return;
        }

        Lat = lat;
        Long = @long;
        LocationName = name;
    }
}

using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;

namespace ODK.Web.Razor.Pages.Groups;

public class IndexModel : OdkPageModel
{
    public double? Distance { get; private set; }

    public DistanceUnitType? DistanceUnit { get; private set; }

    public double? Lat { get; private set; }

    public double? Long { get; private set; }

    public string? LocationName { get; private set; }

    public string? TopicGroup { get; private set; }

    public void OnGet(
        [FromQuery(Name = "l")] string? latLong,
        [FromQuery(Name = "n")] string? name,
        [FromQuery(Name = "d")] double? distance,
        [FromQuery(Name = "u")] DistanceUnitType? unit,
        [FromQuery(Name = "c")] string? topicGroup)
    {
        TopicGroup = topicGroup;
        UpdateDistance(distance, unit);
        UpdateLocation(latLong, name);
    }

    private void UpdateDistance(double? distance, DistanceUnitType? unit)
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

        /* Invariant, matching how the pair is written - by LatLong.ToString and by the picker's script.
           Not the ambient culture: that is the default locale rather than the request's, so a comma
           decimal separator configured there would stop the site reading back what it just wrote. */
        if (!double.TryParse(latLongParts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) ||
            !double.TryParse(latLongParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var @long))
        {
            return;
        }

        Lat = lat;
        Long = @long;
        LocationName = name;
    }
}
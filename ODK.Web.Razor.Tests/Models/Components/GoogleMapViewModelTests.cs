using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Countries;
using ODK.Web.Razor.Models.Components;

namespace ODK.Web.Razor.Tests.Models.Components;

[Parallelizable]
public static class GoogleMapViewModelTests
{
    [Test]
    public static void GoogleMapsQuery_CoordinatesOnly_IsLatitudeThenLongitude()
    {
        // Arrange - what a location recorded before place lookup was the source has instead of a place.
        var model = new GoogleMapViewModel
        {
            LatLong = new LatLong(53.3811, -1.4701)
        };

        // Act / Assert
        model.GoogleMapsQuery().Should().Be("53.3811,-1.4701");
    }

    /// <summary>
    /// The query goes into the embed URL, where a decimal comma would read as extra coordinates and the
    /// map would open somewhere else entirely.
    /// </summary>
    [SetCulture("de-DE")]
    [Test]
    public static void GoogleMapsQuery_CultureWithDecimalComma_UsesDecimalPoints()
    {
        // Arrange
        var model = new GoogleMapViewModel
        {
            LatLong = new LatLong(53.3811, -1.4701)
        };

        // Act / Assert
        model.GoogleMapsQuery().Should().Be("53.3811,-1.4701");
    }

    [Test]
    public static void GoogleMapsQuery_ExternalIdAndCoordinates_PrefersThePlace()
    {
        // Arrange - a place id points the embed at the place itself rather than at a point near it.
        var model = new GoogleMapViewModel
        {
            ExternalId = "abc123",
            LatLong = new LatLong(53.3811, -1.4701)
        };

        // Act / Assert
        model.GoogleMapsQuery().Should().Be("place_id:abc123");
    }

    [Test]
    public static void GoogleMapsQuery_NothingKnown_IsNull()
    {
        // Arrange - the embed is not rendered at all rather than pointed at nowhere.
        var model = new GoogleMapViewModel();

        // Act / Assert
        model.GoogleMapsQuery().Should().BeNull();
    }
}

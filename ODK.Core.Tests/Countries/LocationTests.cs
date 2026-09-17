using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Venues;

namespace ODK.Core.Tests.Countries;

/// <summary>
/// The three entities that store a position as two doubles and expose it as a <see cref="LatLong"/>. They
/// are covered together because the mistake worth catching is the same one in each: reading the wrong field
/// into the pair, which no caller can see and which every downstream distance silently believes.
/// </summary>
[Parallelizable]
public static class LocationTests
{
    /* Latitude, longitude, and a value that is neither. Transposing the two, or reading one of them twice,
       only shows up where the two differ - a fixture at 50,50 passes whatever the property does. The
       northern hemisphere and a western longitude also make the signs differ, so dropping one is caught. */
    private const double Latitude = 53.3811;
    private const double Longitude = -1.4701;

    [TestCaseSource(nameof(Locations))]
    public static void LatLong_ReadsLatitudeAndLongitudeInOrder(ILocation location)
    {
        // Act
        var result = location.LatLong;

        // Assert
        result.Lat.Should().Be(Latitude);
        result.Long.Should().Be(Longitude);
    }

    [TestCaseSource(nameof(Locations))]
    public static void LatLong_DoesNotRepeatOneCoordinate(ILocation location)
    {
        // Act - the failure this guards is a pair built from one field twice, which reads as a plausible
        // position and puts everything on the 45-degree line through the equator.
        var result = location.LatLong;

        // Assert
        result.Lat.Should().NotBe(result.Long);
    }

    private static IEnumerable<TestCaseData> Locations()
    {
        yield return new TestCaseData(
            new ChapterLocation { Latitude = Latitude, Longitude = Longitude, Name = "Sheffield" })
            .SetName("ChapterLocation");

        yield return new TestCaseData(
            new MemberLocation { Latitude = Latitude, Longitude = Longitude, Name = "Sheffield" })
            .SetName("MemberLocation");

        yield return new TestCaseData(
            new VenueLocation { Latitude = Latitude, Longitude = Longitude, Name = "Sheffield" })
            .SetName("VenueLocation");
    }
}

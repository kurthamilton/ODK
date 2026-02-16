using ODK.Core.Countries;

namespace ODK.Core.Members;

public class MemberLocation : ILocation
{
    public Guid? CountryId { get; set; }

    public double Latitude { get; set; }

    public LatLong LatLong => new LatLong(Latitude, Longitude);

    public double Longitude { get; set; }

    public Guid MemberId { get; set; }

    public string Name { get; set; } = string.Empty;

    public double DistanceFrom(ILocation other, DistanceUnit unit)
        => LatLong.DistanceFrom(other.LatLong, unit);
}

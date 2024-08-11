using ODK.Core.Countries;

namespace ODK.Core.Members;

public class MemberLocation : ILocation
{    
    public LatLong LatLong { get; set; }

    public Guid MemberId { get; set; }

    public string Name { get; set; } = "";

    public double DistanceFrom(ILocation other, DistanceUnit unit)
        => LatLong.DistanceFrom(other.LatLong, unit);
}

using ODK.Core.Countries;

namespace ODK.Core.Members;

public class MemberLocation
{    
    public LatLong? LatLong { get; set; }

    public Guid MemberId { get; set; }

    public string? Name { get; set; }
}

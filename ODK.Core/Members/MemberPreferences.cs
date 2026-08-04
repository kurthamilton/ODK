using ODK.Core.Countries;

namespace ODK.Core.Members;

public class MemberPreferences
{
    public DistanceUnitType? DistanceUnit { get; set; }

    public Guid MemberId { get; set; }
}
using ODK.Core.Countries;

namespace ODK.Core.Members;

public class MemberPreferences
{
    public DistanceUnit? DistanceUnit { get; set; }

    public Guid? DistanceUnitId { get; set; }

    public Guid MemberId { get; set; }
}

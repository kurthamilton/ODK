using ODK.Core.Countries;

namespace ODK.Core.Members;

public class MemberPreferences
{
    public DistanceUnit? DistanceUnit { get; set; }

    public Guid? DistanceUnitId { get; set; }

    public Guid MemberId { get; set; }

    public MemberPreferences Clone()
    {
        return new MemberPreferences
        {
            DistanceUnit = DistanceUnit,
            DistanceUnitId = DistanceUnitId,
            MemberId = MemberId
        };
    }
}

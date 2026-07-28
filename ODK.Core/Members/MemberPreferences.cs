using ODK.Core.Countries;

namespace ODK.Core.Members;

public class MemberPreferences
{
    public DistanceUnitType? DistanceUnit { get; set; }

    /// <summary>
    /// The member's preferred formatting locale (a culture name, e.g. "en-GB"), or null to fall back to
    /// their country's default. Date/time/number formatting is derived from this. No UI sets it yet.
    /// </summary>
    public string? Locale { get; set; }

    public Guid MemberId { get; set; }
}
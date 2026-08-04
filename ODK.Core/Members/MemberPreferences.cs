using ODK.Core.Countries;

namespace ODK.Core.Members;

public class MemberPreferences
{
    public DistanceUnitType? DistanceUnit { get; set; }

    /// <summary>
    /// The member's preferred formatting locale (a specific culture name, e.g. "en-GB"), captured from the
    /// request locale on their first request. Used to format request-independent output (emails,
    /// notifications) for them; null falls back to the default locale.
    /// </summary>
    public string? Locale { get; set; }

    public Guid MemberId { get; set; }
}
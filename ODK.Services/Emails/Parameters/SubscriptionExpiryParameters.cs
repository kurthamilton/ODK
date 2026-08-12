using System.Globalization;
using ODK.Core.Members;
using ODK.Core.Utils;

namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Warns a member that their membership or trial is about to lapse, or has lapsed.
/// </summary>
/// <remarks>
/// One class across the four expiry types. They are all sent from the same method with the same
/// values; only the template differs, which is the part an admin edits.
/// </remarks>
public sealed class SubscriptionExpiryParameters : EmailTypeParameters
{
    private const string DisabledDateName = "subscription.disabledDate";

    private const string ExpiryDateName = "subscription.expiryDate";

    private const string FirstNameName = "member.firstName";

    private readonly CultureInfo _culture;
    private readonly Member _member;
    private readonly TimeZoneInfo _timeZone;

    public SubscriptionExpiryParameters(Member member, CultureInfo culture)
    {
        _culture = culture;
        _member = member;
        _timeZone = member.TimeZone;
    }

    public static IReadOnlyCollection<string> Names { get; } =
    [
        FirstNameName,
        ExpiryDateName,
        DisabledDateName
    ];

    public required DateTime? DisabledUtc { get; set; }

    public required DateTime? ExpiresUtc { get; set; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        var dateStringOptions = new FriendlyDateStringOptions
        {
            IncludeDayOfWeek = true,
            TimeZone = _timeZone,
            Culture = _culture
        };

        Add(values, FirstNameName, _member.FirstName);
        Add(values, ExpiryDateName, ExpiresUtc?.ToFriendlyDateString(dateStringOptions));
        Add(values, DisabledDateName, DisabledUtc?.ToFriendlyDateString(dateStringOptions));
    }
}

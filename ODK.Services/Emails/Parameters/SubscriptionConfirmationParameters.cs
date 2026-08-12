using System.Globalization;
using ODK.Core.Countries;
using ODK.Core.Extensions;
using ODK.Core.Members;

namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Confirms a member's payment for a group subscription.
/// </summary>
public sealed class SubscriptionConfirmationParameters : EmailTypeParameters
{
    private const string AmountName = "subscription.amount";

    private const string EndName = "subscription.end";

    private readonly CultureInfo _culture;
    private readonly Currency _currency;
    private readonly Member _member;

    public SubscriptionConfirmationParameters(Currency currency, Member member, CultureInfo culture)
    {
        _culture = culture;
        _currency = currency;
        _member = member;
    }

    public static IReadOnlyCollection<string> Names { get; } = [AmountName, EndName];

    public required decimal Amount { get; set; }

    public required DateTime ExpiresUtc { get; set; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, AmountName, _currency.ToAmountString(Amount));
        Add(values, EndName, _member.ToLocalTime(ExpiresUtc).ToString("d MMMM yyyy", _culture));
    }
}

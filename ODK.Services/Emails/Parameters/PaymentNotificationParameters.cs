using ODK.Core.Countries;

namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Tells a member their payment has gone through.
/// </summary>
public sealed class PaymentNotificationParameters : EmailTypeParameters
{
    private const string AmountName = "payment.amount";

    private const string ReferenceName = "payment.reference";

    private readonly Currency _currency;

    public PaymentNotificationParameters(Currency currency)
    {
        _currency = currency;
    }

    public static IReadOnlyCollection<string> Names { get; } = [AmountName, ReferenceName];

    public required decimal Amount { get; set; }

    public required string Reference { get; set; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, AmountName, _currency.ToAmountString(Amount));
        Add(values, ReferenceName, Reference);
    }
}

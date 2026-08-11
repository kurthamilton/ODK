namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Confirms a member's payment for a group subscription.
/// </summary>
public sealed class SubscriptionConfirmationParameters : EmailTypeParameters
{
    private const string AmountName = "subscription.amount";

    private const string EndName = "subscription.end";

    public static IReadOnlyCollection<string> Names { get; } = [AmountName, EndName];

    public string? Amount { get; set; }

    public string? End { get; set; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, AmountName, Amount);
        Add(values, EndName, End);
    }
}

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

    public static IReadOnlyCollection<string> Names { get; } =
    [
        FirstNameName,
        ExpiryDateName,
        DisabledDateName
    ];

    public string? DisabledDate { get; set; }

    public string? ExpiryDate { get; set; }

    public string? FirstName { get; set; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, FirstNameName, FirstName);
        Add(values, ExpiryDateName, ExpiryDate);
        Add(values, DisabledDateName, DisabledDate);
    }
}

namespace ODK.Services.Payments;

/// <summary>
/// The provider account one platform transacts as. Config states it, so a deployment reaches the account
/// its own configuration names rather than one a restored database happens to point at.
/// </summary>
public class PaymentPlatformSettings
{
    /// <inheritdoc cref="Models.StripePaymentAccount.AccountId"/>
    public required string AccountId { get; init; }

    /// <summary>
    /// Whether anything can be bought on this platform. Read it rather than assuming an account exists: a
    /// plan that costs nothing stays usable while payments are off.
    /// </summary>
    public required bool Enabled { get; init; }

    /// <summary>
    /// The provider key the checkout page hands the provider's own browser script. Public by design -
    /// it identifies the account and authorises nothing.
    /// </summary>
    public required string PublicApiKey { get; init; }
}

using ODK.Core.Payments;

namespace ODK.Services.Settings.Models;

public class SitePaymentSettingsCreateModel
{
    public required string ApiPublicKey { get; init; }

    public required string ApiSecretKey { get; init; }

    public required decimal Commission { get; init; }

    public required bool Enabled { get; init; }

    public required string? ExternalId { get; init; }

    public required string? ExternalUrl { get; init; }

    public required string Name { get; init; }

    /// <summary>
    /// Stated only on creation, and deliberately absent from
    /// <see cref="SitePaymentSettingsUpdateModel"/>: the provider decides how the keys are read and
    /// everything already transacted through the settings was transacted under it, so an existing row
    /// cannot change hands.
    /// </summary>
    public required PaymentProviderType Provider { get; init; }
}

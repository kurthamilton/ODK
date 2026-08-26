namespace ODK.Services.Settings.Models;

public class SitePaymentSettingsUpdateModel
{
    public required string ApiPublicKey { get; init; }

    public required string ApiSecretKey { get; init; }

    public required decimal Commission { get; init; }

    public required bool Enabled { get; init; }

    public required string? ExternalId { get; init; }

    public required string? ExternalUrl { get; init; }

    public required string Name { get; init; }
}

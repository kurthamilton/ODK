namespace ODK.Services.Chapters;

public class UpdateChapterPaymentSettings
{
    public required string? ApiPublicKey { get; init; }

    public required string? ApiSecretKey { get; init; }

    public required Guid? CurrencyId { get; init; }

    public required string? Provider { get; init; }
}

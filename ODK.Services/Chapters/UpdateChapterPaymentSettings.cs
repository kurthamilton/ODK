namespace ODK.Services.Chapters;

public class UpdateChapterPaymentSettings
{
    public required string? ApiPublicKey { get; set; }

    public required string? ApiSecretKey { get; set; }

    public required string? Provider { get; set; }
}

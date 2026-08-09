namespace ODK.Infrastructure.Settings;

public class ReoonSettings
{
    /// <summary>
    /// Empty in environments without a key (local, e2e), where verification simply doesn't run and the
    /// format check stands alone.
    /// </summary>
    public required string ApiKey { get; init; }

    /// <summary>"quick" or "power" - see the integration's settings for what each covers.</summary>
    public required string Mode { get; init; }

    public required string VerifyUrl { get; init; }
}

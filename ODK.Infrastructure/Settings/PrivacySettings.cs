namespace ODK.Infrastructure.Settings;

/// <summary>
/// The values the privacy policy states that are not fixed by the code around them - who operates the site,
/// where a data protection request goes, and how long data is kept.
/// </summary>
/// <remarks>
/// Configuration rather than markup because a policy is a statement of what the app does, so a period stated
/// in it has to be the period the app enforces. Both readings come from here: <c>Program.cs</c> configures
/// Serilog's retention from <see cref="Logging"/>, and the page renders the same value.
/// </remarks>
public class PrivacySettings
{
    public required string HostingProvider { get; init; }

    public required PrivacyLoggingSettings Logging { get; init; }

    public required Dictionary<PlatformKey, PrivacyPlatformSettings> Platforms { get; init; }

    public required string TraderName { get; init; }

    public required string TradingAddress { get; init; }
}

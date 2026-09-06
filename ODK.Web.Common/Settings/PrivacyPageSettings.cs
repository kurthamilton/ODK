namespace ODK.Web.Common.Settings;

/// <summary>
/// What the privacy page needs from <c>Privacy</c> configuration, mapped in <c>DependencyRegistrar</c>.
/// The per-platform entry is already resolved to the platform this deployment serves.
/// </summary>
public class PrivacyPageSettings
{
    public required int BetterStackRetentionDays { get; init; }

    public required string EmailAddress { get; init; }

    public required string HostingProvider { get; init; }

    public required int InviteRetentionDays { get; init; }

    public required int LogRetentionDays { get; init; }

    public required string TraderName { get; init; }

    public required string TradingAddress { get; init; }
}

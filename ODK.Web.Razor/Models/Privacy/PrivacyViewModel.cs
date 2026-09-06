using ODK.Core.Platforms;

namespace ODK.Web.Razor.Models.Privacy;

public class PrivacyViewModel
{
    public required int BetterStackRetentionDays { get; init; }

    public required string ContactEmailAddress { get; init; }

    public required string HostingProvider { get; init; }

    public required DateOnly LastUpdated { get; init; }

    public required int LogRetentionDays { get; init; }

    public required PlatformType Platform { get; init; }

    public required string PlatformName { get; init; }

    public required string TraderName { get; init; }

    public required string TradingAddress { get; init; }
}

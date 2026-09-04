using ODK.Core.Platforms;

namespace ODK.Infrastructure.Settings;

public static class PlatformKeyExtensions
{
    /// <summary>The label configuration names a platform by.</summary>
    /// <remarks>
    /// The inverse of <see cref="ToPlatformType"/>, and the pair have to agree - a section is keyed by the
    /// label while the platform a deployment serves is stated as the app spells it, so selecting an entry
    /// crosses from one vocabulary to the other. <c>PlatformConfigTests</c> round-trips every member.
    /// </remarks>
    public static PlatformKey ToPlatformKey(this PlatformType platform) => platform switch
    {
        PlatformType.Default => PlatformKey.GS,
        PlatformType.DrunkenKnitwits => PlatformKey.DK,
        _ => throw new ArgumentOutOfRangeException(nameof(platform), $"Unsupported platform: {platform}")
    };

    /// <summary>The platform a configuration label names.</summary>
    public static PlatformType ToPlatformType(this PlatformKey key) => key switch
    {
        PlatformKey.DK => PlatformType.DrunkenKnitwits,
        PlatformKey.GS => PlatformType.Default,
        _ => throw new ArgumentOutOfRangeException(nameof(key), $"Unsupported platform key: {key}")
    };
}

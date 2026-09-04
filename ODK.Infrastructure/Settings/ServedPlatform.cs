using ODK.Core.Platforms;

namespace ODK.Infrastructure.Settings;

/// <summary>
/// The platform a deployment serves, and what a per-platform section of its configuration states for it.
/// </summary>
/// <remarks>
/// One definition, because several sections are keyed by platform and every one of them has to select the
/// same entry: a deployment logging to one platform's directory while serving the other's groups would be
/// harder to spot than either mistake alone.
/// </remarks>
public static class ServedPlatform
{
    /* Config states the platform a deployment serves, and the binder yields None where it does not. The
       older platform is the one an unnamed deployment is. */
    public static PlatformType Of(AppSettings appSettings) => appSettings.Platform != PlatformType.None
        ? appSettings.Platform
        : PlatformType.DrunkenKnitwits;

    /// <summary>
    /// What a per-platform section states for this deployment, falling back to the entry labelled GS the way
    /// every other per-platform lookup falls back to <see cref="PlatformType.Default"/>, the platform GS names.
    /// </summary>
    public static T Of<T>(AppSettings appSettings, IReadOnlyDictionary<PlatformKey, T> platforms)
    {
        var key = Of(appSettings).ToPlatformKey();

        return platforms.TryGetValue(key, out var value)
            ? value
            : platforms[PlatformKey.GS];
    }
}

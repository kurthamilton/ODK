using ODK.Core.Platforms;

namespace ODK.Services.Platforms;

public class PlatformProviderSettings
{
    /// <summary>
    /// Every platform's canonical base URL, not just this deployment's: work whose platform is decided by
    /// what it is about rather than by the request that triggered it has to be able to name another site.
    /// </summary>
    public required IReadOnlyDictionary<PlatformType, string> BaseUrls { get; init; }

    public required IReadOnlyDictionary<PlatformType, string> Names { get; init; }

    public required PlatformType Platform { get; init; }
}

using ODK.Core.Platforms;

namespace ODK.Services.Platforms;

public class PlatformProviderSettings
{
    public required IReadOnlyDictionary<PlatformType, string> Names { get; init; }

    public required IReadOnlyDictionary<PlatformType, IReadOnlyCollection<string>> Urls { get; init; }
}

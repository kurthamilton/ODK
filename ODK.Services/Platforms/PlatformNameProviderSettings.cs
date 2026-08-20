using ODK.Core.Platforms;

namespace ODK.Services.Platforms;

public class PlatformNameProviderSettings
{
    public required IReadOnlyDictionary<PlatformType, string> Names { get; init; }
}

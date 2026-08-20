using ODK.Core.Platforms;

namespace ODK.Services.Platforms;

public class PlatformNameProvider : IPlatformNameProvider
{
    private readonly PlatformNameProviderSettings _settings;

    public PlatformNameProvider(PlatformNameProviderSettings settings)
    {
        _settings = settings;
    }

    public string GetName(PlatformType platform) => _settings.Names.TryGetValue(platform, out var name)
        ? name
        : _settings.Names[PlatformType.Default];
}

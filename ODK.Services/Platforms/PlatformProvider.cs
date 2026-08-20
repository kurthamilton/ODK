using ODK.Core.Platforms;

namespace ODK.Services.Platforms;

public class PlatformProvider : IPlatformProvider
{
    private readonly PlatformProviderSettings _settings;

    public PlatformProvider(PlatformProviderSettings settings)
    {
        _settings = settings;
    }

    public string GetName(PlatformType platform) => _settings.Names.TryGetValue(platform, out var name)
        ? name
        : _settings.Names[PlatformType.Default];

    public PlatformType GetPlatform(string requestUrl)
    {
        foreach (var key in _settings.Urls.Keys)
        {
            var urls = _settings.Urls[key];
            if (urls.Any(x => requestUrl.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
            {
                return key;
            }
        }

        return PlatformType.Default;
    }
}
using ODK.Core.Platforms;
using ODK.Services.Exceptions;

namespace ODK.Services.Platforms;

public class PlatformProvider : IPlatformProvider
{
    private readonly PlatformProviderSettings _settings;

    public PlatformProvider(PlatformProviderSettings settings)
    {
        _settings = settings;
    }

    /* The first configured URL, which is the canonical one - a platform may carry more so that an alternate
       host still resolves to it in GetPlatform, but only one can be the site a link points at. No fallback to
       another platform's URL: a link to the wrong site is worse than a job that fails naming the gap, and the
       caller is expected to resolve this before it commits to anything. */
    public string GetBaseUrl(PlatformType platform)
    {
        var url = _settings.Urls.TryGetValue(platform, out var urls)
            ? urls.FirstOrDefault()
            : null;

        return url ?? throw new OdkServiceException($"No base URL configured for platform {platform}");
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
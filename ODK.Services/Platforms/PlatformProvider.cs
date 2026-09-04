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

    public PlatformType Platform => _settings.Platform;

    /* No fallback to another platform's URL: a link to the wrong site is worse than a job that fails naming
       the gap, and the caller is expected to resolve this before it commits to anything. */
    public string GetBaseUrl(PlatformType platform)
    {
        var url = _settings.BaseUrls.TryGetValue(platform, out var baseUrl)
            ? baseUrl
            : null;

        return !string.IsNullOrWhiteSpace(url)
            ? url
            : throw new OdkServiceException($"No base URL configured for platform {platform}");
    }

    public string GetName(PlatformType platform) => _settings.Names.TryGetValue(platform, out var name)
        ? name
        : _settings.Names[PlatformType.Default];
}

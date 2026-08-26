using ODK.Core.Platforms;

namespace ODK.Services.Platforms;

public interface IPlatformProvider
{
    /// <summary>
    /// The platform's canonical base URL, for work that has to name a site without a request to read one
    /// from. Throws when the platform has none configured.
    /// </summary>
    string GetBaseUrl(PlatformType platform);

    string GetName(PlatformType platform);

    PlatformType GetPlatform(string requestUrl);
}
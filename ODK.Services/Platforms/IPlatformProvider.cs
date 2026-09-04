using ODK.Core.Platforms;

namespace ODK.Services.Platforms;

public interface IPlatformProvider
{
    /// <summary>
    /// The platform this deployment serves, stated in its configuration. Fixed for the life of the process:
    /// a site is bound to one platform's domains, so nothing about a request can change the answer.
    /// </summary>
    PlatformType Platform { get; }

    /// <summary>
    /// The platform's canonical base URL, for work that has to name a site without a request to read one
    /// from. Throws when the platform has none configured.
    /// </summary>
    string GetBaseUrl(PlatformType platform);

    string GetName(PlatformType platform);
}

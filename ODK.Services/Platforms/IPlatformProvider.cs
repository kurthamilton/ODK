using ODK.Core.Platforms;

namespace ODK.Services.Platforms;

public interface IPlatformProvider
{
    string GetBaseUrl();

    PlatformType GetPlatform();
}

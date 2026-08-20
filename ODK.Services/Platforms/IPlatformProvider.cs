using ODK.Core.Platforms;

namespace ODK.Services.Platforms;

public interface IPlatformProvider
{
    string GetName(PlatformType platform);

    PlatformType GetPlatform(string requestUrl);
}
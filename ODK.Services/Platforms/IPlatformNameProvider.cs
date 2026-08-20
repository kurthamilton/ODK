using ODK.Core.Platforms;

namespace ODK.Services.Platforms;

public interface IPlatformNameProvider
{
    string GetName(PlatformType platform);
}

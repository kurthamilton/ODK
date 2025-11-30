namespace ODK.Core.Platforms;

public interface IPlatformProvider
{
    PlatformType GetPlatform();

    PlatformType GetPlatform(string requestUrl);
}

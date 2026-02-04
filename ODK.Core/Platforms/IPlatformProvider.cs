namespace ODK.Core.Platforms;

public interface IPlatformProvider
{
    PlatformType GetPlatform(string requestUrl);
}
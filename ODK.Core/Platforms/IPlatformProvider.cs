namespace ODK.Core.Platforms;

public interface IPlatformProvider
{
    string GetBaseUrl();

    PlatformType GetPlatform();
}

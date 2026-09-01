using ODK.Core.Emails;
using ODK.Core.Platforms;

namespace ODK.Services.Emails;

public class SiteEmailSettingsProvider : ISiteEmailSettingsProvider
{
    private readonly SiteEmailSettingsProviderSettings _settings;

    public SiteEmailSettingsProvider(SiteEmailSettingsProviderSettings settings)
    {
        _settings = settings;
    }

    public SiteEmailSettings Get(PlatformType platform) =>
        _settings.Platforms.TryGetValue(platform, out var settings)
            ? settings
            : _settings.Platforms[PlatformType.Default];
}

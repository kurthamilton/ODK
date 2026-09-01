using ODK.Core.Emails;
using ODK.Core.Platforms;

namespace ODK.Services.Emails;

public class SiteEmailSettingsProviderSettings
{
    public required IReadOnlyDictionary<PlatformType, SiteEmailSettings> Platforms { get; init; }
}

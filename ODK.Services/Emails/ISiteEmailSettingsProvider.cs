using ODK.Core.Emails;
using ODK.Core.Platforms;

namespace ODK.Services.Emails;

public interface ISiteEmailSettingsProvider
{
    SiteEmailSettings Get(PlatformType platform);
}

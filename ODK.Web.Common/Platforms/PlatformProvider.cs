using System;
using System.Linq;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Platforms;

public class PlatformProvider : IPlatformProvider
{
    private readonly PlatformProviderSettings _settings;

    public PlatformProvider(PlatformProviderSettings settings)
    {
        _settings = settings;
    }

    public PlatformType GetPlatform(string requestUrl)
    {
        return _settings.DrunkenKnitwitsBaseUrls.Any(x => requestUrl.StartsWith(x, StringComparison.OrdinalIgnoreCase))
            ? PlatformType.DrunkenKnitwits
            : PlatformType.Default;
    }
}
using System;
using System.Linq;
using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Web.Common.Platforms;

public class PlatformProvider : IPlatformProvider
{
    private static PlatformType? _platform;

    private readonly IHttpRequestProvider _httpRequestProvider;
    private readonly PlatformProviderSettings _settings;

    public PlatformProvider(PlatformProviderSettings settings, 
        IHttpRequestProvider httpRequestProvider)
    {
        _httpRequestProvider = httpRequestProvider;
        _settings = settings;
    }

    public string GetBaseUrl()
    {
        var platform = GetPlatform();

        return platform switch
        {
            PlatformType.DrunkenKnitwits => _settings.DrunkenKnitwitsBaseUrls.First(),
            _ => _settings.DefaultBaseUrls.First()
        };
    }

    public PlatformType GetPlatform()
    {
        if (_platform != null)
        {
            return _platform.Value;
        }

        var url = _httpRequestProvider.RequestUrl;

        if (_settings.DrunkenKnitwitsBaseUrls.Any(x => url.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
        {
            _platform = PlatformType.DrunkenKnitwits;
        }
        else
        {
            _platform = PlatformType.Default;
        }

        return _platform.Value;
    }
}

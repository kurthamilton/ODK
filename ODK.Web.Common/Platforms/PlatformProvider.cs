using ODK.Core.Platforms;
using ODK.Core.Web;
using System;
using System.Linq;

namespace ODK.Web.Common.Platforms;

public class PlatformProvider : IPlatformProvider
{
    private static PlatformType? _platform;

    private readonly IHttpRequestContextProvider _httpRequestProvider;
    private readonly PlatformProviderSettings _settings;

    public PlatformProvider(PlatformProviderSettings settings, 
        IHttpRequestContextProvider httpRequestProvider)
    {
        _httpRequestProvider = httpRequestProvider;
        _settings = settings;
    }

    public PlatformType GetPlatform()
    {
        if (_platform != null)
        {
            return _platform.Value;
        }

        var httpRequestContext = _httpRequestProvider.Get();
        return GetPlatform(httpRequestContext.RequestUrl);
    }

    public PlatformType GetPlatform(string requestUrl)
    {
        if (_platform != null)
        {
            return _platform.Value;
        }

        if (_settings.DrunkenKnitwitsBaseUrls.Any(x => requestUrl.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
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

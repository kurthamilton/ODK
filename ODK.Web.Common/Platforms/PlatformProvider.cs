using System;
using System.Linq;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services.Platforms;

namespace ODK.Web.Common.Platforms;

public class PlatformProvider : IPlatformProvider
{
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
        var url = _httpRequestProvider.RequestUrl;

        if (_settings.DrunkenKnitwitsBaseUrls.Any(x => url.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
        {
            return PlatformType.DrunkenKnitwits;
        }

        return PlatformType.Default;
    }
}

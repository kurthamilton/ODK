using System;
using System.Linq;
using ODK.Core.Platforms;
using ODK.Web.Common.Services;

namespace ODK.Web.Common.Platforms;

public class PlatformProvider : IPlatformProvider
{
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
        var httpRequestContext = _httpRequestProvider.Get();
        return GetPlatform(httpRequestContext.RequestUrl);
    }

    public PlatformType GetPlatform(string requestUrl)
    {
        return _settings.DrunkenKnitwitsBaseUrls.Any(x => requestUrl.StartsWith(x, StringComparison.OrdinalIgnoreCase))
            ? PlatformType.DrunkenKnitwits
            : PlatformType.Default;
    }
}
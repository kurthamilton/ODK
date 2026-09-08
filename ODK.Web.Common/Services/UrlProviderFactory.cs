using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Services;
using ODK.Services.Platforms;
using ODK.Services.Web;
using ODK.Web.Common.Routes;

namespace ODK.Web.Common.Services;

public class UrlProviderFactory : IUrlProviderFactory
{
    private readonly IOdkRoutesFactory _odkRoutesFactory;
    private readonly IPlatformProvider _platformProvider;

    public UrlProviderFactory(IOdkRoutesFactory odkRoutesFactory, IPlatformProvider platformProvider)
    {
        _odkRoutesFactory = odkRoutesFactory;
        _platformProvider = platformProvider;
    }

    public IUrlProvider Create(IServiceRequest request, Chapter? chapter)
        => Create(chapter?.Platform ?? request.Platform);

    /* The factories are passed on rather than resolved here, because the platform a URL is built against is
       decided per URL by what it is about - see UrlProvider. */
    public IUrlProvider Create(PlatformType platform)
        => new UrlProvider(platform, _odkRoutesFactory, _platformProvider);
}

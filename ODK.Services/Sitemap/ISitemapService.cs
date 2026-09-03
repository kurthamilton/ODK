using ODK.Services.Sitemap.ViewModels;

namespace ODK.Services.Sitemap;

public interface ISitemapService
{
    Task<SitemapViewModel> GetSitemapViewModel(IServiceRequest request);
}

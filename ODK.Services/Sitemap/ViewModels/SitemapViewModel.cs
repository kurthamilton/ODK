namespace ODK.Services.Sitemap.ViewModels;

public class SitemapViewModel
{
    public required IReadOnlyCollection<SitemapChapterViewModel> Chapters { get; init; }
}

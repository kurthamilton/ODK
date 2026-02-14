using ODK.Services.Chapters;
using ODK.Web.Razor.Models.Sitemap;

namespace ODK.Web.Razor.Pages;

public class SitemapModel : OdkPageModel
{
    private readonly Lazy<string> _baseUrl;
    private readonly IChapterService _chapterService;

    public SitemapModel(IChapterService chapterService)
    {
        _baseUrl = new(() => ServiceRequest.HttpRequestContext.BaseUrl);
        _chapterService = chapterService;
    }

    public string BaseUrl => _baseUrl.Value;

    public async Task<IReadOnlyCollection<SitemapNodeModel>> GetSitemapNodes()
    {
        var nodes = new List<SitemapNodeModel>
        {
            new()
            {
                Url = GetUrl("/")
            }
        };

        var chapters = await _chapterService.GetApprovedChapters(Platform);
        chapters = chapters
            .OrderBy(x => x.GetDisplayName(Platform))
            .ToArray();

        foreach (var chapter in chapters)
        {
            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Groups.Group(chapter))
            });

            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Account.Join(chapter))
            });

            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Groups.Events(chapter))
            });

            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Groups.Contact(chapter))
            });

            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Groups.About(chapter))
            });
        }

        return nodes;
    }

    private string GetUrl(string path) => $"{BaseUrl}{path}";
}
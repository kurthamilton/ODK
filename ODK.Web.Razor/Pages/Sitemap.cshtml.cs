using ODK.Core.Utils;
using ODK.Services.Chapters;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Sitemap;

namespace ODK.Web.Razor.Pages;

public class SitemapModel : OdkPageModel
{
    private readonly IChapterService _chapterService;
    
    public SitemapModel(IChapterService chapterService)
    {
        _chapterService = chapterService;

        BaseUrl = UrlUtils.BaseUrl(HttpRequestContext.RequestUrl);
    }

    public string BaseUrl { get; }

    public async Task<IReadOnlyCollection<SitemapNodeModel>> GetSitemapNodes()
    {
        var nodes = new List<SitemapNodeModel>
        {
            new()
            {
                Url = GetUrl("/")
            }
        };

        var chaptersDto = await _chapterService.GetChaptersDto(Platform);
        var chapters = chaptersDto
            .Chapters
            .Where(x => x.Approved())
            .OrderBy(x => x.Name);

        foreach (var chapter in chapters)
        {
            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Groups.Group(Platform, chapter))
            });

            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Account.Join(chapter))
            });

            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Groups.Events(Platform, chapter))
            });

            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Groups.Contact(Platform, chapter))
            });

            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Groups.About(Platform, chapter))
            });
        }

        return nodes;
    }

    private string GetUrl(string path) => $"{BaseUrl}{path}";
}

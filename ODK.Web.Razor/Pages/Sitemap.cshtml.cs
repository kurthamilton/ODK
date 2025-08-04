using System.Globalization;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services.Chapters;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Sitemap;

namespace ODK.Web.Razor.Pages;

public class SitemapModel : PageModel
{
    private readonly IChapterService _chapterService;
    private readonly IPlatformProvider _platformProvider;
    
    public SitemapModel(
        IUrlProvider urlProvider,
        IChapterService chapterService,
        IPlatformProvider platformProvider)
    {
        _chapterService = chapterService;
        _platformProvider = platformProvider;

        BaseUrl = urlProvider.BaseUrl();
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

        var platform = _platformProvider.GetPlatform();
        var chaptersDto = await _chapterService.GetChaptersDto();
        var chapters = chaptersDto
            .Chapters
            .Where(x => x.Approved())
            .OrderBy(x => x.Name);

        foreach (var chapter in chapters)
        {
            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Groups.Group(platform, chapter))
            });

            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Groups.Join(platform, chapter))
            });

            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Groups.Events(platform, chapter))
            });

            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Groups.Contact(platform, chapter))
            });

            nodes.Add(new SitemapNodeModel
            {
                Url = GetUrl(OdkRoutes.Groups.Questions(platform, chapter))
            });
        }

        return nodes;
    }

    private string GetUrl(string path) => $"{BaseUrl}{path}";
}

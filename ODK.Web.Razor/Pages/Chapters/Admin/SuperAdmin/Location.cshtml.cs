using ODK.Services.Caching;
using ODK.Services.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SuperAdmin;

public class LocationModel : ChapterSuperAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public LocationModel(IRequestCache requestCache, IChapterAdminService chapterAdminService)
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }
}

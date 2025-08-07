using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SuperAdmin;

public class EmailsModel : ChapterSuperAdminPageModel
{
    public EmailsModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }
}

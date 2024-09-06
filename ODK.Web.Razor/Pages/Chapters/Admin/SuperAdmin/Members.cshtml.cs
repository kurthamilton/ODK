using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SuperAdmin;

public class MembersModel : ChapterSuperAdminPageModel
{
    public MembersModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }
}

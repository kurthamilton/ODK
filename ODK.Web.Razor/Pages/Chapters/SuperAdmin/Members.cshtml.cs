using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin;

public class MembersModel : SuperAdminPageModel
{
    public MembersModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }
}

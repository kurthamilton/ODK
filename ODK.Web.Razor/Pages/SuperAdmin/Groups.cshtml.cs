using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class GroupsModel : SuperAdminPageModel
{
    public GroupsModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }
}

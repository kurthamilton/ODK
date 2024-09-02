using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class MessagesModel : SuperAdminPageModel
{
    public MessagesModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}

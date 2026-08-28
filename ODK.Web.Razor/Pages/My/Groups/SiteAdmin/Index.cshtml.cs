using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages.My.Groups.SiteAdmin;

public class IndexModel : GroupSiteAdminPageModel
{
    public IActionResult OnGet() => Redirect(OdkRoutes.GroupAdmin.SiteAdminMembers(Chapter).Path);
}

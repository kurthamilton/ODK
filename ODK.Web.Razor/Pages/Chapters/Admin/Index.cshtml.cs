using Microsoft.AspNetCore.Mvc;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin;

/// <summary>
/// The chapter admin landing page is a pure redirect: it sends the member to the first admin page
/// their role permits. It has no content of its own, so which page that is depends entirely on the
/// member — an Organiser and an Owner land in different places.
/// </summary>
public class IndexModel : AdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Any;

    public async Task<IActionResult> OnGetAsync()
    {
        var adminMember = await RequestStore.GetCurrentChapterAdminMember();
        var route = AdminRoutes.LandingRoute(Chapter, adminMember, CurrentMember);
        return route != null
            ? Redirect(route.Path)
            : Redirect(OdkRoutes.Groups.Group(Chapter));
    }
}

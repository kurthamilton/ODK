using Microsoft.AspNetCore.Authorization;
using ODK.Services.Authentication;

namespace ODK.Web.Razor.Pages.Chapters.Admin;

[Authorize(Roles = OdkRoles.Admin)]
public abstract class AdminPageModel : OdkPageModel
{
}
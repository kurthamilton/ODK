using Microsoft.AspNetCore.Authorization;

namespace ODK.Web.Razor.Pages.My.Groups;

/// <summary>
/// Base class for all /my/groups/* pages
/// </summary>
[Authorize]
public abstract class OdkGroupAdminPageModel : OdkPageModel
{    
}
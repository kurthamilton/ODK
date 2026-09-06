using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages.Groups.Group;

/// <summary>
/// Where an invite's decline link lands on this platform. Anonymous, because the member it names may not
/// be able to sign in: the account an import raised for them has no password until they accept.
/// </summary>
public class RefuseInviteModel : OdkGroupPageModel
{
    public string? InviteToken { get; private set; }

    public void OnGet([FromQuery] string? token) => InviteToken = token;
}

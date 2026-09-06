using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages.Chapters;

public class RefuseInviteModel : OdkPageModel
{
    public string? InviteToken { get; private set; }

    public void OnGet([FromQuery] string? token) => InviteToken = token;
}

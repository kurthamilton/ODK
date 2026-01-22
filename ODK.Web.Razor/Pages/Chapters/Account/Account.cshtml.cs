using Microsoft.AspNetCore.Authorization;

namespace ODK.Web.Razor.Pages.Chapters.Account;

[Authorize]
public class AccountModel : OdkPageModel
{
}
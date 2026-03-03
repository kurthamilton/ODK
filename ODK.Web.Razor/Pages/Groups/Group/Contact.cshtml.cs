using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages.Groups.Group;

public class ContactModel : OdkGroupPageModel
{
    public IActionResult OnGet()
    {
        if (CurrentMemberOrDefault != null)
        {
            return Redirect(OdkRoutes.Groups.Conversations(Chapter));
        }

        return Page();
    }
}
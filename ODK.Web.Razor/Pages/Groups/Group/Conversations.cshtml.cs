using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages.Groups.Group;

public class ConversationsModel : OdkGroupPageModel
{
    public bool Archived { get; private set; }

    public IActionResult OnGet(bool archived = false)
    {
        if (CurrentMemberOrDefault == null)
        {
            return Redirect(OdkRoutes.Groups.Contact(Chapter));
        }

        Archived = archived;

        return Page();
    }
}
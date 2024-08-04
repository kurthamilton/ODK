using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages.Chapters.Members;

public class MemberModel : ChapterPageModel2
{
    public Guid MemberId { get; private set; }

    public IActionResult OnGet(Guid id)
    {
        MemberId = id;
        return Page();
    }
}

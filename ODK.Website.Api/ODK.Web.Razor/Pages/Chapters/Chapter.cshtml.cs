using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters
{
    public class ChapterModel : ChapterPageModel
    {
        public ChapterModel(IRequestCache requestCache)
            : base(requestCache)
        {
        }

        public IActionResult OnGet()
        {
            if (Chapter == null)
            {
                return RedirectToPage("/");
            }

            return Page();
        }
    }
}

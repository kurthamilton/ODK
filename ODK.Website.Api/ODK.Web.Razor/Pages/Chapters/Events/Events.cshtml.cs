using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Events
{
    public class EventsModel : ChapterPageModel
    {
        public EventsModel(IRequestCache requestCache)
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

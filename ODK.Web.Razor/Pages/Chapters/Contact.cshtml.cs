using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Web.Razor.Models.Contact;

namespace ODK.Web.Razor.Pages.Chapters;

public class ContactModel : ChapterPageModel
{
    private readonly IChapterService _chapterService;

    public ContactModel(IRequestCache requestCache, IChapterService chapterService) 
        : base(requestCache)
    {
        _chapterService = chapterService;
    }

    public bool Sent { get; private set; }

    public void OnGet(bool sent)
    {
        Sent = sent;
    }

    public async Task<IActionResult> OnPostAsync(ContactFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _chapterService.SendContactMessage(Chapter.Id,
            viewModel.EmailAddress ?? "",
            viewModel.Message ?? "",
            viewModel.Recaptcha ?? "");

        return Redirect($"/{Chapter.Name}/Contact?Sent=True");
    }
}

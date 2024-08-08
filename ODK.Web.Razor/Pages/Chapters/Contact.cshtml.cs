using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Contact;
using ODK.Web.Razor.Models.Contact;

namespace ODK.Web.Razor.Pages.Chapters;

public class ContactModel : ChapterPageModel
{
    private readonly IContactService _contactService;

    public ContactModel(IRequestCache requestCache, IContactService contactService) 
        : base(requestCache)
    {
        _contactService = contactService;
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

        await _contactService.SendChapterContactMessage(Chapter,
            viewModel.EmailAddress ?? "",
            viewModel.Message ?? "",
            viewModel.Recaptcha ?? "");

        return Redirect($"/{Chapter.Name}/Contact?Sent=True");
    }
}

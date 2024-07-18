using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Web.Common.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Contact;

namespace ODK.Web.Razor.Pages.Chapters;

public class ContactModel : ChapterPageModel2<ContactPageViewModel>
{
    private readonly IChapterService _chapterService;
    private readonly IChapterWebService _chapterWebService;

    public ContactModel(IChapterWebService chapterWebService, 
        IChapterService chapterService)
    {
        _chapterService = chapterService;
        _chapterWebService = chapterWebService;
    }

    [FromQuery]
    public bool Sent { get; private set; }

    public async Task<IActionResult> OnPostAsync(ContactFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var chapter = await _chapterService.GetChapter(Name);

        await _chapterService.SendContactMessage(chapter,
            viewModel.EmailAddress ?? "",
            viewModel.Message ?? "",
            viewModel.Recaptcha ?? "");

        AddFeedback(new FeedbackViewModel("Your message has been sent. Thank you for getting in touch.", 
            FeedbackType.Success));

        return Redirect($"/{chapter.Name}/Contact");
    }

    protected override Task<ContactPageViewModel> GetViewModelAsync() 
        => _chapterWebService.GetContactPageViewModelAsync(MemberId, Name);
}

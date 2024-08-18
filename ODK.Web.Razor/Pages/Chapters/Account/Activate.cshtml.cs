using Microsoft.AspNetCore.Mvc;
using ODK.Services.Authentication;
using ODK.Services.Caching;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Account;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class ActivateModel : ChapterPageModel2
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IRequestCache _requestCache;


    public ActivateModel(IAuthenticationService authenticationService,
        IRequestCache requestCache) 
    {
        _authenticationService = authenticationService;
        _requestCache = requestCache;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string token, ActivateFormViewModel viewModel)
    {
        var chapter = await _requestCache.GetChapterAsync(ChapterName);
        var result = await _authenticationService.ActivateChapterAccountAsync(chapter.Id, token, viewModel.Password);
        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback("Your account has been activated. You can now login.", FeedbackType.Success);
        return Redirect($"/{ChapterName}/Account/Login");
    }
}

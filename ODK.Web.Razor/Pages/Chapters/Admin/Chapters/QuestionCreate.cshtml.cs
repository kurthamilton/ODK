using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class QuestionCreateModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public QuestionCreateModel(IRequestCache requestCache, IChapterAdminService chapterAdminService) 
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(ChapterQuestionFormViewModel viewModel)
    {
        ServiceResult result = await _chapterAdminService.CreateChapterQuestion(CurrentMemberId, Chapter.Id, 
            new CreateChapterQuestion(viewModel.Answer, viewModel.Question));

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Question created", FeedbackType.Success));
        return Redirect($"/{Chapter.Name}/Admin/Chapter/Questions");
    }
}

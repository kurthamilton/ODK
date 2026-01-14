using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class QuestionCreateModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public QuestionCreateModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(ChapterQuestionFormViewModel viewModel)
    {
        var serviceRequest = await CreateMemberChapterServiceRequest();
        var result = await _chapterAdminService.CreateChapterQuestion(serviceRequest,
            new CreateChapterQuestion
            {
                Answer = viewModel.Answer ?? "",
                Name = viewModel.Question ?? ""
            });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        var chapter = await GetChapter();
        AddFeedback(new FeedbackViewModel("Question created", FeedbackType.Success));
        return Redirect($"/{chapter.ShortName}/Admin/Chapter/Questions");
    }
}
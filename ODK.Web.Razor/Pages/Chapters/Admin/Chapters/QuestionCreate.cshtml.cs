using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Security;
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

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Questions;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(ChapterQuestionFormViewModel viewModel)
    {
        var serviceRequest = MemberChapterAdminServiceRequest;
        var result = await _chapterAdminService.CreateChapterQuestion(serviceRequest,
            new CreateChapterQuestion
            {
                Answer = viewModel.Answer ?? "",
                Name = viewModel.Question ?? ""
            });

        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        var chapter = Chapter;
        AddFeedback("Question created", FeedbackType.Success);
        return Redirect($"/{chapter.ShortName}/Admin/Chapter/Questions");
    }
}
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class QuestionEditModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public QuestionEditModel(IRequestCache requestCache, IChapterAdminService chapterAdminService)
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    public ChapterQuestion Question { get; private set; } = null!;

    public async Task<IActionResult> OnGet(Guid id)
    {
        var serviceRequest = await GetAdminServiceRequest();
        Question = await _chapterAdminService.GetChapterQuestion(serviceRequest, id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, ChapterQuestionFormViewModel viewModel)
    {
        var serviceRequest = await GetAdminServiceRequest();
        var result = await _chapterAdminService.UpdateChapterQuestion(serviceRequest, id,
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

        AddFeedback(new FeedbackViewModel("Question updated", FeedbackType.Success));
        return Redirect($"/{Chapter.ShortName}/Admin/Chapter/Questions");
    }
}

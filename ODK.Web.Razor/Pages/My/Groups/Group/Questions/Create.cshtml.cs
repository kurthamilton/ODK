using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.My.Groups.Group.Questions;

public class CreateModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public CreateModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(ChapterQuestionFormViewModel model)
    {
        var serviceRequest = MemberChapterServiceRequest(ChapterId);
        
        var result = await _chapterAdminService.CreateChapterQuestion(serviceRequest, new CreateChapterQuestion
        {
            Name = model.Question ?? string.Empty,
            Answer = model.Answer ?? string.Empty
        });

        AddFeedback(result, "Question created");

        if (!result.Success)
        {
            return Page();
        }

        var chapter = await _chapterAdminService.GetChapter(serviceRequest);
        return Redirect(OdkRoutes.MemberGroups.GroupQuestions(Platform, chapter));
    }
}

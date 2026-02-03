using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Security;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.My.Groups.Group.Questions;

public class CreateModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Questions;

    public CreateModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(ChapterQuestionFormViewModel model)
    {
        var serviceRequest = MemberChapterAdminServiceRequest;

        var result = await _chapterAdminService.CreateChapterQuestion(serviceRequest, new ChapterQuestionCreateModel
        {
            Name = model.Question ?? string.Empty,
            Answer = model.Answer ?? string.Empty
        });

        AddFeedback(result, "Question created");

        if (!result.Success)
        {
            return Page();
        }

        return Redirect(OdkRoutes.GroupAdmin.Questions(Chapter).Path);
    }
}
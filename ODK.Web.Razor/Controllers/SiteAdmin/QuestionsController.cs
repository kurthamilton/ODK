using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Authentication;
using ODK.Services.Questions;
using ODK.Services.Questions.Models;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Controllers.Admin;
using ODK.Web.Razor.Models.Feedback;
using ODK.Web.Razor.Models.SiteAdmin;

namespace ODK.Web.Razor.Controllers.SiteAdmin;

[Authorize(Roles = OdkRoles.SiteAdmin)]
public class QuestionsController : AdminControllerBase
{
    private readonly ISiteQuestionAdminService _siteQuestionAdminService;

    public QuestionsController(
        ISiteQuestionAdminService siteQuestionAdminService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _siteQuestionAdminService = siteQuestionAdminService;
    }

    [HttpPost("/siteadmin/questions/new")]
    public async Task<IActionResult> Create([FromForm] SiteQuestionFormViewModel viewModel)
    {
        var result = await _siteQuestionAdminService.CreateQuestion(MemberServiceRequest, ToModel(viewModel));
        if (!result.Success)
        {
            AddFeedback(result);
            return RedirectToReferrer();
        }

        AddFeedback("Question created", FeedbackType.Success);
        return Redirect(OdkRoutes.SiteAdmin.Questions.Path);
    }

    [HttpPost("/siteadmin/questions/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _siteQuestionAdminService.DeleteQuestion(MemberServiceRequest, id);
        AddFeedback("Question deleted", FeedbackType.Success);
        return Redirect(OdkRoutes.SiteAdmin.Questions.Path);
    }

    [HttpPost("/siteadmin/questions/{id:guid}/move/down")]
    public async Task<IActionResult> MoveDown(Guid id)
    {
        await _siteQuestionAdminService.UpdateQuestionDisplayOrder(MemberServiceRequest, id, moveBy: 1);
        return RedirectToReferrer();
    }

    [HttpPost("/siteadmin/questions/{id:guid}/move/up")]
    public async Task<IActionResult> MoveUp(Guid id)
    {
        await _siteQuestionAdminService.UpdateQuestionDisplayOrder(MemberServiceRequest, id, moveBy: -1);
        return RedirectToReferrer();
    }

    [HttpPost("/siteadmin/questions/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] SiteQuestionFormViewModel viewModel)
    {
        var result = await _siteQuestionAdminService.UpdateQuestion(
            MemberServiceRequest, id, ToModel(viewModel));
        AddFeedback(result, "Question updated");
        return RedirectToReferrer();
    }

    private static SiteQuestionUpdateModel ToModel(SiteQuestionFormViewModel viewModel) => new()
    {
        AnswerHtml = viewModel.AnswerHtml,
        Name = viewModel.Question
    };
}

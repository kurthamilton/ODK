using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Security;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models.Admin.Chapters;
using ODK.Web.Razor.Models.Chapters;
using ODK.Web.Razor.Models.Topics;

namespace ODK.Web.Razor.Controllers.Admin;

[Authorize]
public class GroupAdminController : OdkControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;

    public GroupAdminController(
        IChapterAdminService chapterAdminService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _chapterAdminService = chapterAdminService;
    }

    [HttpPost("admin/groups/{chapterId:guid}/conversations")]
    public async Task<IActionResult> StartConversation(Guid chapterId,
        [FromForm] ChapterAdminStartConversationFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Conversations, MemberChapterServiceRequest);
        await _chapterAdminService.StartConversation(request, viewModel.MemberId,
            viewModel.Subject ?? string.Empty, viewModel.Message ?? string.Empty);
        AddFeedback("Message sent", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{chapterId:guid}/conversations/{conversationId:guid}/reply")]
    public async Task<IActionResult> ReplyToConversation(Guid chapterId, Guid conversationId,
        [FromForm] ChapterConversationReplyFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Conversations, MemberChapterServiceRequest);
        var result = await _chapterAdminService.ReplyToConversation(
            request, conversationId, viewModel.Message ?? string.Empty);
        AddFeedback(result, "Reply sent");
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{chapterId:guid}/description")]
    public async Task<IActionResult> UpdateDescription(Guid chapterId, [FromForm] string description)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Texts, MemberChapterServiceRequest);
        await _chapterAdminService.UpdateChapterDescription(request, description);
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{chapterId:guid}/questions/{questionId:guid}")]
    public async Task<IActionResult> UpdateQuestion(
        Guid chapterId, Guid questionId, [FromForm] ChapterQuestionFormViewModel viewModel)
    {
        var securable = OdkRoutes.GroupAdmin.Question(Chapter, questionId).Securable;
        var request = MemberChapterAdminServiceRequest.Create(
            securable, MemberChapterServiceRequest);
        var result = await _chapterAdminService.UpdateChapterQuestion(request, questionId,
            new CreateChapterQuestion
            {
                Answer = viewModel.Answer ?? string.Empty,
                Name = viewModel.Question ?? string.Empty
            });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return RedirectToReferrer();
        }

        AddFeedback("Question updated", FeedbackType.Success);

        return Redirect(OdkRoutes.GroupAdmin.Questions(Chapter).Path);
    }

    [HttpPost("admin/groups/{chapterId:guid}/texts/register")]
    public async Task<IActionResult> UpdateRegisterText(Guid chapterId, [FromForm] string text)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Texts, MemberChapterServiceRequest);
        await _chapterAdminService.UpdateChapterDescription(request, text);
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{chapterId:guid}/topics")]
    public async Task<IActionResult> UpdateTopics(Guid chapterId, [FromForm] TopicPickerViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.Topics, MemberChapterServiceRequest);
        var result = await _chapterAdminService.UpdateChapterTopics(request, viewModel.TopicIds ?? []);
        AddFeedback(result, "Topics updated");
        return RedirectToReferrer();
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Chapters;
using ODK.Web.Razor.Models.Chapters;
using ODK.Web.Razor.Models.Topics;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers.Admin;

[Authorize]
public class GroupAdminController : OdkControllerBase
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IRequestStore _requestStore;

    public GroupAdminController(
        IChapterAdminService chapterAdminService,
        IRequestStore requestStore)
        : base(requestStore)
    {
        _chapterAdminService = chapterAdminService;
        _requestStore = requestStore;
    }

    [HttpPost("admin/groups/{chapterId:guid}/conversations")]
    public async Task<IActionResult> StartConversation(Guid chapterId,
        [FromForm] ChapterAdminStartConversationFormViewModel viewModel)
    {
        var request = CreateMemberChapterServiceRequest(chapterId);
        await _chapterAdminService.StartConversation(request, viewModel.MemberId,
            viewModel.Subject ?? string.Empty, viewModel.Message ?? string.Empty);
        AddFeedback("Message sent", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{chapterId:guid}/conversations/{conversationId:guid}/reply")]
    public async Task<IActionResult> ReplyToConversation(Guid chapterId, Guid conversationId,
        [FromForm] ChapterConversationReplyFormViewModel viewModel)
    {
        var request = CreateMemberChapterServiceRequest(chapterId);
        var result = await _chapterAdminService.ReplyToConversation(request, conversationId, viewModel.Message ?? string.Empty);
        AddFeedback(result, "Reply sent");
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{chapterId:guid}/description")]
    public async Task<IActionResult> UpdateDescription(Guid chapterId, [FromForm] string description)
    {
        var request = CreateMemberChapterServiceRequest(chapterId);
        await _chapterAdminService.UpdateChapterDescription(request, description);
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{chapterId:guid}/questions/{questionId:guid}")]
    public async Task<IActionResult> UpdateQuestion(
        Guid chapterId, Guid questionId, [FromForm] ChapterQuestionFormViewModel viewModel)
    {
        var request = CreateMemberChapterServiceRequest(chapterId);
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

        var chapter = await _requestStore.GetChapter();
        var redirectUrl = OdkRoutes.MemberGroups.GroupQuestions(Platform, chapter);
        return Redirect(redirectUrl);
    }

    [HttpPost("admin/groups/{chapterId:guid}/texts/register")]
    public async Task<IActionResult> UpdateRegisterText(Guid chapterId, [FromForm] string text)
    {
        var request = CreateMemberChapterServiceRequest(chapterId);
        await _chapterAdminService.UpdateChapterDescription(request, text);
        return RedirectToReferrer();
    }

    [HttpPost("admin/groups/{chapterId:guid}/topics")]
    public async Task<IActionResult> UpdateTopics(Guid chapterId, [FromForm] TopicPickerViewModel viewModel)
    {
        var request = CreateMemberChapterServiceRequest(chapterId);
        var result = await _chapterAdminService.UpdateChapterTopics(request, viewModel.TopicIds ?? []);
        AddFeedback(result, "Topics updated");
        return RedirectToReferrer();
    }
}
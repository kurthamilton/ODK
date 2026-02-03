using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Security;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class AdminMemberModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public AdminMemberModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public Guid MemberId { get; private set; }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.AdminMembers;

    public void OnGet(Guid id)
    {
        MemberId = id;
    }

    public async Task<IActionResult> OnPostAsync(Guid id, AdminMemberFormViewModel viewModel)
    {
        var serviceRequest = MemberChapterAdminServiceRequest;
        var result = await _chapterAdminService.UpdateChapterAdminMember(serviceRequest, id, new ChapterAdminMemberUpdateModel
        {
            AdminEmailAddress = viewModel.AdminEmailAddress,
            ReceiveContactEmails = viewModel.ReceiveContactEmails,
            ReceiveEventCommentEmails = viewModel.ReceiveEventCommentEmails,
            ReceiveNewMemberEmails = viewModel.ReceiveNewMemberEmails,
            Role = viewModel.Role ?? default
        });

        if (result.Success)
        {
            AddFeedback("Chapter admin member updated", FeedbackType.Success);
            return Redirect(AdminRoutes.AdminMembers(Chapter).Path);
        }

        AddFeedback(result);
        return Page();
    }
}
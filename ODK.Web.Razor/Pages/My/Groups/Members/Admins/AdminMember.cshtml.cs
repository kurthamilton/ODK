using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.My.Groups.Members.Admins;

public class AdminMemberModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public AdminMemberModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public Guid MemberId { get; private set; }

    public void OnGet(Guid memberId)
    {
        MemberId = memberId;
    }

    public async Task<IActionResult> OnPostAsync(Guid memberId, AdminMemberFormViewModel viewModel)
    {
        var result = await _chapterAdminService.UpdateChapterAdminMember(AdminServiceRequest, memberId, new UpdateChapterAdminMember
        {
            AdminEmailAddress = viewModel.AdminEmailAddress,
            ReceiveContactEmails = viewModel.ReceiveContactEmails,
            ReceiveEventCommentEmails = viewModel.ReceiveEventCommentEmails,
            ReceiveNewMemberEmails = viewModel.ReceiveNewMemberEmails
        });

        AddFeedback(result, "Admin member updated");

        if (!result.Success)
        {
            return Page();
        }

        var chapter = await GetChapter();
        return Redirect(OdkRoutes.MemberGroups.MemberAdmins(Platform, chapter));
    }
}
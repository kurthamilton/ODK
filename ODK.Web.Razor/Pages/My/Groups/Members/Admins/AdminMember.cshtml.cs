using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Security;
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

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.AdminMembers;

    public void OnGet(Guid memberId)
    {
        MemberId = memberId;
    }

    public async Task<IActionResult> OnPostAsync(Guid memberId, AdminMemberFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest;
        var result = await _chapterAdminService.UpdateChapterAdminMember(request, memberId, new ChapterAdminMemberUpdateModel
        {
            AdminEmailAddress = viewModel.AdminEmailAddress,
            ReceiveContactEmails = viewModel.ReceiveContactEmails,
            ReceiveEventCommentEmails = viewModel.ReceiveEventCommentEmails,
            ReceiveNewMemberEmails = viewModel.ReceiveNewMemberEmails,
            Role = viewModel.Role ?? default
        });

        AddFeedback(result, "Admin member updated");

        if (!result.Success)
        {
            return Page();
        }

        var path = OdkRoutes.GroupAdmin.AdminMembers(Chapter).Path;
        return Redirect(path);
    }
}
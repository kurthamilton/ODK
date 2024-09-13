using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.My.Groups.Members.Admins;

public class AdminMemberModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IPlatformProvider _platformProvider;

    public AdminMemberModel(IChapterAdminService chapterAdminService,
        IPlatformProvider platformProvider)
    {
        _chapterAdminService = chapterAdminService;
        _platformProvider = platformProvider;
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
            ReceiveNewMemberEmails = viewModel.ReceiveNewMemberEmails,
            SendNewMemberEmails = viewModel.SendNewMemberEmails
        });

        AddFeedback(result, "Admin member updated");

        if (!result.Success)
        {
            return Page();            
        }                

        var platform = _platformProvider.GetPlatform();
        var chapter = await _chapterAdminService.GetChapter(AdminServiceRequest);
        return Redirect(OdkRoutes.MemberGroups.MemberAdmins(platform, chapter));
    }
}

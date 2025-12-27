using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class AdminMemberModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public AdminMemberModel(IRequestCache requestCache, IChapterAdminService chapterAdminService) 
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    public Guid MemberId { get; private set; }

    public void OnGet(Guid id)
    {
        MemberId = id;
    }

    public async Task<IActionResult> OnPostAsync(Guid id, AdminMemberFormViewModel viewModel)
    {
        var serviceRequest = await GetAdminServiceRequest();
        var result = await _chapterAdminService.UpdateChapterAdminMember(serviceRequest, id, new UpdateChapterAdminMember
        {
            AdminEmailAddress = viewModel.AdminEmailAddress,
            ReceiveContactEmails = viewModel.ReceiveContactEmails,
            ReceiveEventCommentEmails = viewModel.ReceiveEventCommentEmails,
            ReceiveNewMemberEmails = viewModel.ReceiveNewMemberEmails,
            SendNewMemberEmails = viewModel.SendNewMemberEmails
        });

        if (result.Success)
        {
            AddFeedback(new FeedbackViewModel("Chapter admin member updated", FeedbackType.Success));
            return Redirect($"/{Chapter.ShortName}/Admin/Members/Admins");
        }

        AddFeedback(new FeedbackViewModel(result));
        return Page();
    }
}

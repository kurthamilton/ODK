using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Security;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class MemberModel : AdminPageModel
{
    private readonly IMemberAdminService _memberAdminService;

    public MemberModel(IMemberAdminService memberAdminService)
    {
        _memberAdminService = memberAdminService;
    }

    public Guid MemberId { get; private set; }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Members;

    public void OnGet(Guid id)
    {
        MemberId = id;
    }

    public async Task<IActionResult> OnPostAsync(Guid id, MemberFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest;
        var result = await _memberAdminService.UpdateMemberSubscription(request, id, new MemberSubscriptionUpdateModel
        {
            ExpiryDate = viewModel.SubscriptionExpiryDate,
            Type = viewModel.SubscriptionType ?? SubscriptionType.None
        });

        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        AddFeedback("Member subscription updated", FeedbackType.Success);
        return Redirect(AdminRoutes.Member(Chapter, id).Path);
    }
}
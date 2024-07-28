using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Members;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class MemberModel : MemberAdminPageModel
{
    public MemberModel(IRequestCache requestCache, IMemberAdminService memberAdminService) 
        : base(requestCache, memberAdminService)
    {
    }
    
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(Guid id, MemberFormViewModel viewModel)
    {
        var result = await MemberAdminService.UpdateMemberSubscription(CurrentMemberId, id, new UpdateMemberSubscription
        {
            ExpiryDate = viewModel.SubscriptionExpiryDate,
            Type = viewModel.SubscriptionType ?? SubscriptionType.None
        });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Member subscription updated", FeedbackType.Success));
        return Redirect($"/{Chapter.Name}/Admin/Members/{id}");
    }
}

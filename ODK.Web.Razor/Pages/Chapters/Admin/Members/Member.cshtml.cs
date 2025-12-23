using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class MemberModel : AdminPageModel
{
    private readonly IMemberAdminService _memberAdminService;

    public MemberModel(IRequestCache requestCache, IMemberAdminService memberAdminService) 
        : base(requestCache)
    {
        _memberAdminService = memberAdminService;
    }

    public Guid MemberId { get; private set; }

    public void OnGet(Guid id)
    {
        MemberId = id;
    }

    public async Task<IActionResult> OnPostAsync(Guid id, MemberFormViewModel viewModel)
    {
        var request = await GetAdminServiceRequest();
        var result = await _memberAdminService.UpdateMemberSubscription(request, id, new UpdateMemberSubscription
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
        return Redirect($"/{Chapter.ShortName}/Admin/Members/{id}");
    }
}

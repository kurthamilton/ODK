using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Members;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Account;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class AccountModel : ChapterPageModel
{
    private readonly IMemberService _memberService;

    public AccountModel(IRequestCache requestCache, IMemberService memberService)
        : base(requestCache)
    {
        _memberService = memberService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(ProfileFormViewModel viewModel)
    {
        UpdateMemberProfile update = new UpdateMemberProfile
        {
            FirstName = viewModel.FirstName,
            LastName = viewModel.LastName,
            Properties = viewModel.Properties.Select(x => new UpdateMemberProperty
            {
                ChapterPropertyId = x.ChapterPropertyId,
                Value = string.Equals(x.Value, "Other", StringComparison.InvariantCultureIgnoreCase) && 
                        !string.IsNullOrEmpty(x.OtherValue) 
                    ? x.OtherValue ?? ""
                    : x.Value ?? ""
            })
        };

        Guid? memberId = User.MemberId();

        ServiceResult result = await _memberService.UpdateMemberProfile(memberId!.Value, update);
        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Profile updated", FeedbackType.Success));
        return RedirectToPage();
    }
}

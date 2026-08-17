using Microsoft.AspNetCore.Mvc;
using ODK.Services.Members;
using ODK.Services.Users.ViewModels;

namespace ODK.Web.Razor.Pages.Groups.Group;

public class JoinModel : OdkGroupPageModel
{
    private readonly IMemberService _memberService;

    public JoinModel(IMemberService memberService)
    {
        _memberService = memberService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost([FromForm] ChapterProfileFormSubmitViewModel viewModel)
    {
        var properties = viewModel.Properties.Select(x => x.ToMemberPropertyUpdate());

        var request = MemberChapterServiceRequest;
        var result = await _memberService.JoinChapter(request, properties);
        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        return Redirect(OdkRoutes.Groups.Group(Chapter));
    }
}
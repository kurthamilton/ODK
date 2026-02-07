using Microsoft.AspNetCore.Mvc;
using ODK.Services.Members;
using ODK.Services.Members.Models;
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

    public async Task<IActionResult> OnPost([FromForm] ChapterProfileFormViewModel viewModel)
    {
        var properties = viewModel.Properties.Select(x => new MemberPropertyUpdateModel
        {
            ChapterPropertyId = x.ChapterPropertyId,
            Value = string.Equals(x.Value, "Other", StringComparison.InvariantCultureIgnoreCase) &&
                        !string.IsNullOrEmpty(x.OtherValue)
            ? x.OtherValue ?? ""
                    : x.Value ?? ""
        });

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
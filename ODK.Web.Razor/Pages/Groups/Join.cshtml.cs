using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Users.ViewModels;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Groups;

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
        var chapter = await GetChapter();

        var properties = viewModel.Properties.Select(x => new UpdateMemberProperty
        {
            ChapterPropertyId = x.ChapterPropertyId,
            Value = string.Equals(x.Value, "Other", StringComparison.InvariantCultureIgnoreCase) &&
                        !string.IsNullOrEmpty(x.OtherValue)
            ? x.OtherValue ?? ""
                    : x.Value ?? ""
        });

        var result = await _memberService.JoinChapter(CreateMemberChapterServiceRequest(chapter.Id), properties);
        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        return Redirect(OdkRoutes.Groups.Group(PlatformType.Default, chapter));
    }
}
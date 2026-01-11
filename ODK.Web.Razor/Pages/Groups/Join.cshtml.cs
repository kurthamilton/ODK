using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;
using ODK.Services.Chapters;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Users.ViewModels;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Pages.Groups;

public class JoinModel : OdkGroupPageModel
{
    private readonly IRequestStore _requestStore;
    private readonly IMemberService _memberService;

    public JoinModel(
        IMemberService memberService,
        IRequestStore requestStore)
    {
        _memberService = memberService;
        _requestStore = requestStore;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost([FromForm] ChapterProfileFormViewModel viewModel)
    {
        var chapter = await _requestStore.GetChapter();

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
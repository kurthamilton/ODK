using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Core.Platforms;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Chapters;

namespace ODK.Web.Razor.Pages.My.Groups;

public class GroupCreateModel : OdkPageModel
{
    private readonly IChapterService _chapterService;
    private readonly IPlatformProvider _platformProvider;

    public GroupCreateModel(IChapterService chapterService,
        IPlatformProvider platformProvider)
    {
        _chapterService = chapterService;
        _platformProvider = platformProvider;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync([FromForm] CreateChapterSubmitViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (viewModel.Location.Lat == null || viewModel.Location.Long == null)
        {
            AddFeedback("Location not set", FeedbackType.Error);
            return Page();
        }

        var result = await _chapterService.CreateChapter(CurrentMemberId, new ChapterCreateModel
        {
            Description = viewModel.Description?.Trim() ?? "",
            Location = new LatLong(viewModel.Location.Lat.Value, viewModel.Location.Long.Value),
            LocationName = viewModel.Location.Name,
            Name = viewModel.Name?.Trim() ?? "",
            TimeZoneId = viewModel.Location.TimeZoneId,
            TopicIds = viewModel.TopicIds
        });

        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        AddFeedback("Group created. Once approved you will be able to publish and start accepting group members.", FeedbackType.Success);

        var platform = _platformProvider.GetPlatform();

        return result.Value != null
            ? Redirect(OdkRoutes2.MemberGroups.Group(platform, result.Value))
            : Redirect(OdkRoutes2.MemberGroups.Index(platform));
    }
}

using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Core.Images;
using ODK.Core.Platforms;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Chapters;

namespace ODK.Web.Razor.Pages.My.Groups;

public class CreateModel : OdkPageModel
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IPlatformProvider _platformProvider;

    public CreateModel(IChapterAdminService chapterAdminService,
        IPlatformProvider platformProvider)
    {
        _chapterAdminService = chapterAdminService;
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

        if (string.IsNullOrEmpty(viewModel.ImageDataUrl))
        {
            AddFeedback("No image provided", FeedbackType.Warning);
            return Page();
        }

        if (!ImageHelper.TryParseDataUrl(viewModel.ImageDataUrl, out var bytes))
        {
            AddFeedback("Image could not be processed", FeedbackType.Error);
            return Page();
        }

        var result = await _chapterAdminService.CreateChapter(CurrentMemberId, new ChapterCreateModel
        {
            Description = viewModel.Description?.Trim() ?? "",
            ImageData = bytes,
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
            ? Redirect(OdkRoutes.MemberGroups.Group(platform, result.Value))
            : Redirect(OdkRoutes.MemberGroups.Index(platform));
    }
}

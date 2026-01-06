using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Core.Images;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Topics.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Chapters;
using ODK.Web.Razor.Models.Topics;

namespace ODK.Web.Razor.Pages.My.Groups;

public class CreateModel : OdkPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public CreateModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(
        [FromForm] CreateChapterSubmitViewModel viewModel,
        [FromForm] TopicPickerFormSubmitViewModel topics)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (viewModel.CountryId == null)
        {
            AddFeedback("Country not set", FeedbackType.Error);
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

        var result = await _chapterAdminService.CreateChapter(MemberServiceRequest, new ChapterCreateModel
        {
            CountryId = viewModel.CountryId.Value,
            Description = viewModel.Description?.Trim() ?? "",
            ImageData = bytes,
            Location = new LatLong(viewModel.Location.Lat.Value, viewModel.Location.Long.Value),
            LocationName = viewModel.Location.LocationName,
            Name = viewModel.Name?.Trim() ?? "",
            NewTopics = topics.NewTopics
                ?.Where((x, i) => !string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(topics.NewTopicGroups![i]))
                ?.Select((x, i) => new NewTopicModel
                {
                    Topic = x ?? "",
                    TopicGroup = topics.NewTopicGroups![i] ?? ""
                })
                .ToArray() ?? [],
            TimeZoneId = viewModel.Location.TimeZoneId,
            TopicIds = viewModel.TopicIds ?? []
        });

        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        AddFeedback("Group created. Once approved you will be able to publish and start accepting group members.", FeedbackType.Success);

        return result.Value != null
            ? Redirect(OdkRoutes.MemberGroups.Group(Platform, result.Value))
            : Redirect(OdkRoutes.MemberGroups.Index(Platform));
    }
}

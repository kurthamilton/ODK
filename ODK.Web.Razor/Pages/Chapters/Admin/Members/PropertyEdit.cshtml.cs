using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class PropertyEditModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public PropertyEditModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public Guid PropertyId { get; set; }

    public IActionResult OnGet(Guid id)
    {
        PropertyId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, ChapterPropertyFormViewModel viewModel)
    {
        var serviceRequest = await CreateMemberChapterServiceRequest();
        var result = await _chapterAdminService.UpdateChapterProperty(serviceRequest, id, new UpdateChapterProperty
        {
            ApplicationOnly = viewModel.ApplicationOnly,
            DisplayName = viewModel.DisplayName,
            HelpText = viewModel.HelpText,
            Label = viewModel.Label,
            Name = viewModel.Name,
            Options = viewModel.Options?.Split(Environment.NewLine),
            Required = viewModel.Required,
            Subtitle = viewModel.Subtitle
        });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        var chapter = await GetChapter();
        AddFeedback(new FeedbackViewModel("Property updated", FeedbackType.Success));
        return Redirect($"/{chapter.ShortName}/Admin/Chapter/Properties");
    }
}
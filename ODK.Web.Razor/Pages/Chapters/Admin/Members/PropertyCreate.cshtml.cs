using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Security;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class PropertyCreateModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public PropertyCreateModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Properties;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(ChapterPropertyFormViewModel viewModel)
    {
        var serviceRequest = MemberChapterAdminServiceRequest;
        var result = await _chapterAdminService.CreateChapterProperty(serviceRequest, new CreateChapterProperty
        {
            ApplicationOnly = viewModel.ApplicationOnly,
            DataType = viewModel.DataType,
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
            AddFeedback(result);
            return Page();
        }

        AddFeedback("Property created", FeedbackType.Success);
        return Redirect(AdminRoutes.MemberProperties(Chapter).Path);
    }
}
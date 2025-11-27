using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.My.Groups.Group.Properties;

public class CreateModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public CreateModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(ChapterPropertyFormViewModel viewModel)
    {
        var result = await _chapterAdminService.CreateChapterProperty(AdminServiceRequest, new CreateChapterProperty
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

        AddFeedback(result, "Property created");

        if (!result.Success)
        {            
            return Page();
        }

        var chapter = await _chapterAdminService.GetChapter(AdminServiceRequest);

        return Redirect(OdkRoutes.MemberGroups.GroupProperties(Platform, chapter));
    }
}

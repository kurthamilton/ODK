using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Services.Security;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.My.Groups.Members.Properties;

public class CreateModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public CreateModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Properties;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(ChapterPropertyFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest;
        var result = await _chapterAdminService.CreateChapterProperty(request, new ChapterPropertyCreateModel
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

        var path = OdkRoutes.GroupAdmin.MemberProperties(Chapter).Path;
        return Redirect(path);
    }
}
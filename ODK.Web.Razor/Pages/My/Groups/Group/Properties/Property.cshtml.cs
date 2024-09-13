using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.My.Groups.Group.Properties;

public class PropertyModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;
    private readonly IPlatformProvider _platformProvider;

    public PropertyModel(IChapterAdminService chapterAdminService,
        IPlatformProvider platformProvider)
    {
        _chapterAdminService = chapterAdminService;
        _platformProvider = platformProvider;
    }

    public Guid PropertyId { get; private set; }

    public void OnGet(Guid propertyId)
    {
        PropertyId = propertyId;
    }

    public async Task<IActionResult> OnPostAsync(Guid propertyId, ChapterPropertyFormViewModel viewModel)
    {
        var result = await _chapterAdminService.UpdateChapterProperty(AdminServiceRequest, propertyId, new UpdateChapterProperty
        {
            ApplicationOnly = viewModel.ApplicationOnly,
            DisplayName = viewModel.DisplayName,
            HelpText = viewModel.HelpText,
            Label = viewModel.Label,
            Name = viewModel.Name,
            Options = viewModel.Options.Split(Environment.NewLine),
            Required = viewModel.Required,
            Subtitle = viewModel.Subtitle
        });

        AddFeedback(result, "Property updated");

        if (!result.Success)
        {            
            return Page();
        }

        var platform = _platformProvider.GetPlatform();
        var chapter = await _chapterAdminService.GetChapter(AdminServiceRequest);

        return Redirect(OdkRoutes.MemberGroups.GroupProperties(platform, chapter));
    }
}

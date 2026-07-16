using Microsoft.AspNetCore.Mvc;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Security;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class ImportMembersModel : AdminPageModel
{
    private readonly IMemberImportPreviewBuilder _previewBuilder;

    public ImportMembersModel(IMemberImportPreviewBuilder previewBuilder)
    {
        _previewBuilder = previewBuilder;
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.MemberImport;

    public MemberImportPreview? Preview { get; private set; }

    public string? Token { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(IFormFile? file)
    {
        var result = await _previewBuilder.Build(MemberChapterAdminServiceRequest, file);

        if (!result.Success || result.Value == null)
        {
            AddFeedback(result);
            return Page();
        }

        Preview = result.Value.Preview;
        Token = result.Value.Token;
        return Page();
    }
}

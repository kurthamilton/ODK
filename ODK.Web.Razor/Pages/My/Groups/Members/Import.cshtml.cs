using Microsoft.AspNetCore.Mvc;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Security;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Pages.My.Groups.Members;

public class ImportModel : OdkGroupAdminPageModel
{
    private readonly IMemberAdminService _memberAdminService;

    public ImportModel(IMemberAdminService memberAdminService)
    {
        _memberAdminService = memberAdminService;
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.MemberImport;

    public MemberImportPreview? Preview { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(IFormFile? file)
    {
        var result = await MemberImportPreviewBuilder.Build(
            _memberAdminService, MemberChapterAdminServiceRequest, file);

        if (!result.Success || result.Value == null)
        {
            AddFeedback(result);
            return Page();
        }

        Preview = result.Value;
        return Page();
    }
}

using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services.Emails;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class EmailModel : AdminPageModel
{
    private readonly IEmailAdminService _emailAdminService;

    public EmailModel(IEmailAdminService emailAdminService)
    {
        _emailAdminService = emailAdminService;
    }

    public bool CanEdit { get; private set; }

    public ChapterEmail Email { get; private set; } = null!;

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Emails;

    public async Task<IActionResult> OnGetAsync(EmailType type)
    {
        var request = MemberChapterAdminServiceRequest;
        var viewModel = await _emailAdminService.GetChapterEmail(request, type);
        CanEdit = viewModel.CanEdit;
        Email = viewModel.Email;
        return Page();
    }
}

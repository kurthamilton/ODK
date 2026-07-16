using ODK.Services;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Services;

public class MemberImportPreviewBuilder : IMemberImportPreviewBuilder
{
    private readonly IMemberAdminService _memberAdminService;
    private readonly IMemberImportStagingService _stagingService;

    public MemberImportPreviewBuilder(
        IMemberAdminService memberAdminService,
        IMemberImportStagingService stagingService)
    {
        _memberAdminService = memberAdminService;
        _stagingService = stagingService;
    }

    public async Task<ServiceResult<MemberImportStagedPreview>> Build(
        IMemberChapterAdminServiceRequest request,
        IFormFile? file)
    {
        var csvResult = CsvFileReader.Read<MemberImportModel>(file);
        if (!csvResult.Success || csvResult.Value == null)
        {
            return ServiceResult<MemberImportStagedPreview>.Failure(csvResult.Message ?? "The file could not be read");
        }

        // Drop rows without an email address (e.g. a missing/blank email column) - they can't be matched
        // or imported, and a null email would otherwise throw when the preview is built.
        var members = csvResult.Value
            .Where(x => !string.IsNullOrWhiteSpace(x.EmailAddress))
            .ToArray();

        if (members.Length == 0)
        {
            return ServiceResult<MemberImportStagedPreview>.Failure(
                "The uploaded file did not contain any rows with an email address");
        }

        var preview = await _memberAdminService.GetMemberImportPreview(request, members);

        // Stage the parsed rows so the confirm step posts only the token, not every row.
        var token = _stagingService.Stage(members);

        return ServiceResult<MemberImportStagedPreview>.Successful(new MemberImportStagedPreview
        {
            Preview = preview,
            Token = token
        });
    }
}

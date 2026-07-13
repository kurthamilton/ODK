using ODK.Services;
using ODK.Services.Members;
using ODK.Services.Members.Models;

namespace ODK.Web.Razor.Services;

/// <summary>
/// Shared logic for the member-import preview step, used by the equivalent page on each platform
/// so that both behave identically. Reads the uploaded CSV and classifies each row.
/// </summary>
public static class MemberImportPreviewBuilder
{
    public static async Task<ServiceResult<MemberImportPreview>> Build(
        IMemberAdminService memberAdminService,
        IMemberChapterAdminServiceRequest request,
        IFormFile? file)
    {
        var csvResult = CsvFileReader.Read<MemberImportModel>(file);
        if (!csvResult.Success || csvResult.Value == null)
        {
            return ServiceResult<MemberImportPreview>.Failure(csvResult.Message ?? "The file could not be read");
        }

        if (csvResult.Value.Count == 0)
        {
            return ServiceResult<MemberImportPreview>.Failure("The uploaded file did not contain any rows");
        }

        var preview = await memberAdminService.GetMemberImportPreview(request, csvResult.Value);
        return ServiceResult<MemberImportPreview>.Successful(preview);
    }
}

using ODK.Services;
using ODK.Services.Csv;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Services;

public class MemberImportPreviewBuilder : IMemberImportPreviewBuilder
{
    private const long MaxBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes =
        ["text/csv", "application/vnd.ms-excel", "text/plain"];

    private readonly ICsvReader _csvReader;
    private readonly IMemberAdminService _memberAdminService;
    private readonly IMemberImportStagingService _stagingService;

    public MemberImportPreviewBuilder(
        ICsvReader csvReader,
        IMemberAdminService memberAdminService,
        IMemberImportStagingService stagingService)
    {
        _csvReader = csvReader;
        _memberAdminService = memberAdminService;
        _stagingService = stagingService;
    }

    public async Task<ServiceResult<MemberImportStagedPreview>> Build(
        IMemberChapterAdminServiceRequest request,
        IFormFile? file)
    {
        if (!ValidateFile(file, out var error))
        {
            return ServiceResult<MemberImportStagedPreview>.Failure(error);
        }

        IReadOnlyCollection<MemberImportModel> parsed;
        using (var stream = file!.OpenReadStream())
        {
            parsed = _csvReader.Read<MemberImportModel>(stream);
        }

        // Drop rows without an email address (e.g. a missing/blank email column) - they can't be matched
        // or imported, and a null email would otherwise throw when the preview is built.
        var members = parsed
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

    private static bool ValidateFile(IFormFile? file, out string error)
    {
        error = string.Empty;

        if (file is null || file.Length == 0)
        {
            error = "No file uploaded";
            return false;
        }

        if (file.Length > MaxBytes)
        {
            error = "File is too large. The maximum allowed size is 5 MB.";
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".csv")
        {
            error = "Only .csv files are allowed.";
            return false;
        }

        if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            error = $"Invalid content type: {file.ContentType}";
            return false;
        }

        return true;
    }
}

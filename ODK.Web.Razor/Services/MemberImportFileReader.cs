using ODK.Services;
using ODK.Services.Csv;
using ODK.Services.Members.Models;

namespace ODK.Web.Razor.Services;

public class MemberImportFileReader : IMemberImportFileReader
{
    private const long MaxBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes =
        ["text/csv", "application/vnd.ms-excel", "text/plain"];

    private readonly ICsvReader _csvReader;

    public MemberImportFileReader(ICsvReader csvReader)
    {
        _csvReader = csvReader;
    }

    public ServiceResult<IReadOnlyCollection<MemberImportCsvRow>> Read(IFormFile? file)
    {
        if (!ValidateFile(file, out var error))
        {
            return ServiceResult<IReadOnlyCollection<MemberImportCsvRow>>.Failure(error);
        }

        IReadOnlyCollection<MemberImportCsvRow> rows;
        using (var stream = file!.OpenReadStream())
        {
            rows = _csvReader.Read<MemberImportCsvRow>(stream);
        }

        return ServiceResult<IReadOnlyCollection<MemberImportCsvRow>>.Successful(rows);
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

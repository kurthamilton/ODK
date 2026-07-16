using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using ODK.Services;

namespace ODK.Web.Razor.Services;

/// <summary>
/// Reads and validates uploaded CSV files. Shared by controllers and page models so that
/// upload validation and parsing behave identically wherever a CSV is accepted.
/// </summary>
public static class CsvFileReader
{
    private static readonly string[] AllowedMimeTypes =
        ["text/csv", "application/vnd.ms-excel", "text/plain"];

    public static ServiceResult<IReadOnlyCollection<T>> Read<T>(IFormFile? file)
    {
        if (!Validate(file, out var error))
        {
            return ServiceResult<IReadOnlyCollection<T>>.Failure(error);
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var stream = file!.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<T>().ToList();
        return ServiceResult<IReadOnlyCollection<T>>.Successful(records);
    }

    private static bool Validate(IFormFile? file, out string error)
    {
        error = string.Empty;

        if (file is null || file.Length == 0)
        {
            error = "No file uploaded";
            return false;
        }

        const long maxBytes = 5 * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            error = "File is too large. The maximum allowed size is 5 MB.";
            return false;
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".csv")
        {
            error = "Only .csv files are allowed.";
            return false;
        }

        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            error = $"Invalid content type: {file.ContentType}";
            return false;
        }

        return true;
    }
}

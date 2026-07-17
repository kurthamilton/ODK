using System.Globalization;
using System.Text;
using CsvHelper.Configuration;
using ODK.Services.Csv;

namespace ODK.Services.Integrations.Csv;

public class CsvReader : ICsvReader
{
    public IReadOnlyCollection<T> Read<T>(Stream stream)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var csv = new CsvHelper.CsvReader(reader, config);

        return csv.GetRecords<T>().ToArray();
    }
}

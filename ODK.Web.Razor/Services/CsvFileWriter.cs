using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace ODK.Web.Razor.Services;

/// <summary>
/// Writes CSV files using CsvHelper. Shared by controllers so that CSV generation - including
/// CSV/formula-injection protection - behaves identically wherever a CSV is produced.
/// </summary>
public static class CsvFileWriter
{
    public static byte[] Write(IReadOnlyCollection<IReadOnlyCollection<string>> rows)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            // Neutralise CSV/formula injection: any field beginning with an injection character
            // (= + - @, tab, CR) is prefixed with a single quote so spreadsheet apps treat it as
            // text rather than executing it as a formula.
            InjectionOptions = InjectionOptions.Escape,
            InjectionEscapeCharacter = '\''
        };

        using var stream = new MemoryStream();

        // No BOM, matching the previous output; StreamWriter/CsvWriter leave the stream open so it
        // can be read after they flush on dispose.
        using (var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true))
        using (var csv = new CsvWriter(writer, config, leaveOpen: true))
        {
            foreach (var row in rows)
            {
                foreach (var field in row)
                {
                    csv.WriteField(field);
                }

                csv.NextRecord();
            }
        }

        return stream.ToArray();
    }
}

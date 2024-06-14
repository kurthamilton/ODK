using System.Text.RegularExpressions;

namespace ODK.Services.Files;

public class CsvService : ICsvService
{
    // https://stackoverflow.com/questions/18144431/regex-to-split-a-csv
    private static readonly Regex SplitValuesRegex = new Regex("(?:^|,)(?=[^\"]|(\")?)\"?((?(1)[^\"]*|[^,\"]*))\"?(?=,|$)", RegexOptions.Compiled);

    public CsvFile ParseCsvFile(string data)
    {
        string[] lines = data
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

        CsvFile file = new CsvFile();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            CsvRow row = i == 0 ? file.Header : file.AddRow();

            MatchCollection matches = SplitValuesRegex.Matches(line);
            foreach (Match match in matches)
            {
                row.AddValue(match.Groups[2].Value);
            }
        }

        return file;
    }

    public CsvFile ParseCsvFile(Stream stream)
    {
        using StreamReader sr = new StreamReader(stream);
        string data = sr.ReadToEnd();
        return ParseCsvFile(data);
    }
}

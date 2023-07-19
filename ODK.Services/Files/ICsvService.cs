using System.IO;

namespace ODK.Services.Files
{
    public interface ICsvService
    {
        CsvFile ParseCsvFile(string data);

        CsvFile ParseCsvFile(Stream stream);
    }
}

using System.Text;

namespace ODK.Web.Razor.Helpers;

/// <summary>
/// Converts a .NET short-date pattern (e.g. "dd/MM/yyyy") into flatpickr's token syntax (e.g. "d/m/Y"),
/// so the date picker can display dates in the viewer's resolved locale. Only the day/month/year tokens
/// are translated; separators and other literals pass through unchanged.
/// </summary>
public static class FlatpickrDateFormat
{
    public static string FromShortDatePattern(string pattern)
    {
        var result = new StringBuilder(pattern.Length);
        var i = 0;

        while (i < pattern.Length)
        {
            var c = pattern[i];
            if (c is 'd' or 'M' or 'y')
            {
                var start = i;
                while (i < pattern.Length && pattern[i] == c)
                {
                    i++;
                }

                result.Append(MapToken(c, i - start));
            }
            else
            {
                result.Append(c);
                i++;
            }
        }

        return result.ToString();
    }

    private static string MapToken(char token, int length) => token switch
    {
        'd' => length >= 4 ? "l" : length == 3 ? "D" : length == 2 ? "d" : "j",
        'M' => length >= 4 ? "F" : length == 3 ? "M" : length == 2 ? "m" : "n",
        'y' => length == 2 ? "y" : "Y",
        _ => new string(token, length)
    };
}

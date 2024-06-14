using System.Text.RegularExpressions;
using ODK.Core.Images;

namespace ODK.Core.Utils;

public static class FileUtils
{
    private static readonly Regex AlphaNumericImageFileNameRegex = new Regex(GetAlphaNumericFileNamePattern(ImageValidator.ValidFileExtensions),
        RegexOptions.Compiled);

    public static string AlphaNumericImageFileName(this string fileName)
    {
        return AlphaNumericImageFileNameRegex.Replace(fileName, "");
    }

    private static string GetAlphaNumericFileNamePattern(IEnumerable<string> extensions)
    {
        string extensionsPattern = string.Join("|", extensions.Select(x => x.Replace(".", "\\.")));
        return $"(?!({extensionsPattern})$)[^a-zA-Z0-9]";
    }
}

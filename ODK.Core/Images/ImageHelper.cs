using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ODK.Core.Images;

public static class ImageHelper
{
    private static readonly Regex Base64Regex = new Regex("^data:(.+);base64,(?<base64>.+)$", RegexOptions.Compiled);

    public static string ToDataUrl(byte[] imageData, string mimeType)
    {
        var base64 = Convert.ToBase64String(imageData);
        return $"data:{mimeType};base64,{base64}";
    }

    public static bool TryParseDataUrl(string? dataUrl, [NotNullWhen(true)] out byte[]? bytes)
    {
        var match = Base64Regex.Match(dataUrl ?? "");
        if (!match.Success)
        {
            bytes = null;
            return false;
        }

        var base64 = match.Groups["base64"].Value;
        bytes = Convert.FromBase64String(base64);
        return true;
    }
}

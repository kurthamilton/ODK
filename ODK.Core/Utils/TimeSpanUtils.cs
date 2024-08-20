using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ODK.Core.Utils;

public class TimeSpanUtils
{
    public static TimeSpan? FromString(string? value, bool includeSeconds = false)
    {
        var format = includeSeconds
            ? @"hh\:mm\:ss"
            : @"hh\:mm";
        return value != null 
            ? TimeSpan.ParseExact(value, format, CultureInfo.InvariantCulture)
            : null;
    }

    public static string ToString(TimeSpan value, bool includeSeconds = false)
    {
        var format = includeSeconds
            ? @"hh\:mm\:ss"
            : @"hh\:mm";
        return value.ToString(format);
    }

    public static string? ToString(TimeSpan? value, bool includeSeconds = false) => value != null
        ? ToString(value.Value, includeSeconds)
        : null;
}

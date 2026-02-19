using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Extensions;

public static class TempDataExtensions
{
    public static void AddFeedback(this ITempDataDictionary tempData, FeedbackViewModel viewModel)
    {
        tempData.TryGetIntValue("FeedbackCount", out var count);

        var key = $"Feedback[{count}]";
        tempData[$"{key}.Message"] = viewModel.Message;
        tempData[$"{key}.Type"] = viewModel.Type;

        tempData["FeedbackCount"] = count + 1;
    }

    public static bool TryGetEnumValue<T>(this ITempDataDictionary tempData, string key, out T value)
        where T : struct, Enum
    {
        if (!tempData.TryGetValue(key, out var raw))
        {
            value = default;
            return false;
        }

        try
        {
            value = raw switch
            {
                T enumVal => enumVal,
                int rawInt => (T)(object)rawInt,
                long rawLong => (T)(object)(int)rawLong,
                string rawString => Enum.TryParse<T>(rawString, ignoreCase: true, out var enumValue)
                    ? enumValue
                    : default,
                _ => default
            };

            return Enum.IsDefined(value);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidCastException or OverflowException)
        {
            value = default;
            return false;
        }
    }

    public static bool TryGetIntValue(
        this ITempDataDictionary tempData, string key, out int value)
    {
        if (!tempData.TryGetStringValue(key, out var raw))
        {
            value = default;
            return false;
        }

        return int.TryParse(raw, out value);
    }

    public static bool TryGetStringValue(
        this ITempDataDictionary tempData, string key, [NotNullWhen(true)] out string? value)
    {
        if (!tempData.TryGetValue(key, out var raw))
        {
            value = default;
            return false;
        }

        value = raw?.ToString() ?? string.Empty;
        return true;
    }
}
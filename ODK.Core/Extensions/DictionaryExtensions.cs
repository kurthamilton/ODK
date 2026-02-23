using System.Diagnostics.CodeAnalysis;

namespace ODK.Core.Extensions;

public static class DictionaryExtensions
{
    public static bool TryGetEnumValue<T>(
        this IDictionary<string, string> source,
        string key,
        [NotNullWhen(true)] out T? value)
        where T : struct, Enum
        => TryGetEnumValue((IReadOnlyDictionary<string, string>)source.AsReadOnly(), key, out value);

    public static bool TryGetEnumValue<T>(
        this IReadOnlyDictionary<string, string> source,
        string key,
        [NotNullWhen(true)] out T? value)
        where T : struct, Enum
    {
        value = default;

        if (!source.TryGetValue(key, out var stringValue) ||
            !Enum.TryParse<T>(stringValue, out var enumValue))
        {
            return false;
        }

        value = enumValue;
        return true;
    }

    public static bool TryGetGuidValue(
        this IReadOnlyDictionary<string, string> source,
        string key,
        [NotNullWhen(true)] out Guid? value)
    {
        value = null;

        if (!source.TryGetValue(key, out var stringValue) ||
            !Guid.TryParse(stringValue, out var guidValue))
        {
            return false;
        }

        value = guidValue;
        return true;
    }

    public static bool TryGetGuidValue(
        this IDictionary<string, string> source,
        string key,
        [NotNullWhen(true)] out Guid? value)
        => TryGetGuidValue((IReadOnlyDictionary<string, string>)source.AsReadOnly(), key, out value);
}
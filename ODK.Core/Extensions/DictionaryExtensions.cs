using System.Diagnostics.CodeAnalysis;

namespace ODK.Core.Extensions;

public static class DictionaryExtensions
{
    public static bool TryGetGuidValue(
        this IDictionary<string, string> source, 
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
}

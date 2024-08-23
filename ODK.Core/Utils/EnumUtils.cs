using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ODK.Core.Utils;

public static class EnumUtils
{
    public static string GetDisplayValue<T>(T value) where T : Enum
    {
        var name = value.ToString();
        var fieldInfo = typeof(T).GetField(name);

        var attrs = fieldInfo?.GetCustomAttributes<DisplayAttribute>(false) as DisplayAttribute[];
        if (attrs == null || attrs.Length == 0)
        {
            return name;
        }

        return attrs[0].Name ?? name;
    }
}

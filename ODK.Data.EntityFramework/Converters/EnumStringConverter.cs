using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ODK.Data.EntityFramework.Converters;

internal class EnumStringConverter<T> : ValueConverter<T, string> where T : struct, Enum
{
    public EnumStringConverter() 
        : base(
            x => x.ToString(), 
            x => Enum.Parse<T>(x, true))
    {
    }
}

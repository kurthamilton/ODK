using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ODK.Data.EntityFramework.Converters;

internal class NullableEnumStringConverter<T> : ValueConverter<T?, string?> where T : struct, Enum
{
    public NullableEnumStringConverter()
        : base(
            x => x != null ? x.ToString() : null,
            x => x != null ? Enum.Parse<T>(x, true) : null)
    {
    }
}

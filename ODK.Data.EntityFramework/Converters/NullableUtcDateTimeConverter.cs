using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ODK.Data.EntityFramework.Converters;

internal class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter()
        : base(
            x => ToProvider(x),
            x => FromProvider(x))
    {
    }

    private static DateTime? FromProvider(DateTime? value)
    {
        return value != null
            ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            : null;
    }

    private static DateTime? ToProvider(DateTime? value)
    {
        return value;
    }
}

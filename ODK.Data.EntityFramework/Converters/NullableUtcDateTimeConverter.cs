using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ODK.Data.EntityFramework.Converters;

internal class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter()
        : base(
            x =>  x != null ? x.Value.ToUniversalTime() : null,
            x => x != null ? DateTime.SpecifyKind(x.Value, DateTimeKind.Utc) : null)
    {
    }
}

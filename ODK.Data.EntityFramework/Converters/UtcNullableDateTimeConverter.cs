using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ODK.Data.EntityFramework.Converters;

internal class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeConverter()
        : base(
            x =>  x != null ? x.Value.ToUniversalTime() : null,
            x => x != null ? DateTime.SpecifyKind(x.Value, DateTimeKind.Utc) : null)
    {
    }
}

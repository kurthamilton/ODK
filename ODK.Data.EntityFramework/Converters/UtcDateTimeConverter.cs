using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ODK.Data.EntityFramework.Converters;

internal class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            x => x,
            x => DateTime.SpecifyKind(x, DateTimeKind.Utc))
    {
    }
}

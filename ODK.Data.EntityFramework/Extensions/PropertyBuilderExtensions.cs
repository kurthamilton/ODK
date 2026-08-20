using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ODK.Data.EntityFramework.Extensions;

internal static class PropertyBuilderExtensions
{
    internal static PropertyBuilder<decimal> IsMoneyType(this PropertyBuilder<decimal> builder)
        => builder.HasColumnType("money");

    internal static PropertyBuilder<decimal?> IsMoneyType(this PropertyBuilder<decimal?> builder)
        => builder.HasColumnType("money");
}

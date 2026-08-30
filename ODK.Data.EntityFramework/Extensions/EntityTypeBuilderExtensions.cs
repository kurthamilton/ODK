using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Data.EntityFramework.Interceptors;

namespace ODK.Data.EntityFramework.Extensions;

internal static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// Maps <paramref name="property"/> to the column it is read from and written to, and declares a second
    /// column carrying a copy of the same value. <see cref="HtmlColumnMirrorInterceptor"/> fills the copy on
    /// every insert and update, so both columns hold the current value while a rename is in flight.
    /// </summary>
    /// <remarks>
    /// Which column a value is read from is the caller's to state, so moving the readers across is a matter
    /// of swapping the two arguments. Both are needed because the deploy applies migrations before it ships
    /// the code, leaving the previous build running against the new schema for about a minute: whichever
    /// column that build writes has to be the one the readers are still on when they move.
    /// <para>
    /// The two carry the same facets, so the column that survives the rename is the shape of the one it
    /// replaces.
    /// </para>
    /// </remarks>
    internal static EntityTypeBuilder<TEntity> DualWriteColumn<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, string?>> property,
        string writesTo,
        string mirrorsTo,
        int? maxLength = null)
        where TEntity : class
    {
        var value = builder.Property(property).HasColumnName(writesTo);
        var mirror = builder
            .Property<string?>(HtmlColumnMirrorInterceptor.MirrorName(value.Metadata.Name))
            .HasColumnName(mirrorsTo);

        if (maxLength != null)
        {
            value.HasMaxLength(maxLength.Value);
            mirror.HasMaxLength(maxLength.Value);
        }

        return builder;
    }
}

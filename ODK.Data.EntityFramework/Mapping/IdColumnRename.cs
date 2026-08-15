using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ODK.Data.EntityFramework.Mapping;

/// <summary>
/// Transitional support for renaming a table's primary key column to <c>Id</c>. Every entity here already
/// exposes the key as <c>Id</c>; only the column is named after the table (<c>CountryId</c>, <c>MemberId</c>,
/// …), so the change is a mapping change plus a migration.
/// </summary>
/// <remarks>
/// <para>
/// Migrations are applied before the new build is deployed, so for the length of a deploy the previous build
/// runs against the new schema. A rename is therefore done in three releases: add the new column and write
/// both, move the key onto it, then drop the old one.
/// </para>
/// <para>
/// A shadow property rather than a second property on the entity: nothing can read the new column back into
/// the entity, so the two can never disagree. Two mapped properties over one field would be read from their
/// columns in an order EF does not define, and a row written by the previous build - which knows only the old
/// column - would arrive with the new one unset and could silently overwrite a good key.
/// </para>
/// <para>
/// All of this goes when the last table has been renamed: the helper, the interceptor that fills it, and the
/// calls below.
/// </para>
/// </remarks>
public static class IdColumnRename
{
    /// <summary>
    /// Name of the shadow property mapped to the new <c>Id</c> column. Not <c>Id</c> itself, which is taken by
    /// the entity's own property - that one still maps to the column being renamed away from.
    /// </summary>
    public const string ShadowPropertyName = "RenamedId";

    /// <summary>
    /// Maps the new <c>Id</c> column as a shadow property, so a save writes the key to both columns.
    /// <see cref="Interceptors.IdColumnRenameInterceptor"/> is what fills it.
    /// </summary>
    public static void HasRenamedIdColumn<T>(this EntityTypeBuilder<T> builder)
        where T : class
        => builder
            .Property<Guid?>(ShadowPropertyName)
            .HasColumnName("Id");
}

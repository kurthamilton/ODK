using Microsoft.EntityFrameworkCore.Migrations;

namespace ODK.Data.EntityFramework.Migrations.Enums;

/// <summary>
/// Runs the <see cref="EnumTableSql"/> statements from a migration.
/// </summary>
/// <remarks>
/// Including dropping the foreign key: <see cref="MigrationBuilder.DropForeignKey"/> does not cover it,
/// because it needs the constraint's exact name and these are matched by relationship precisely so that
/// one created by hand under another name still counts. See <see cref="EnumTableSql.DropForeignKey{T}"/>.
/// </remarks>
public static class MigrationBuilderExtensions
{
    public static MigrationBuilder AddEnumForeignKey<T>(this MigrationBuilder migrationBuilder, string table, string column)
        where T : struct, Enum
        => Sql(migrationBuilder, EnumTableSql.AddForeignKey<T>(table, column));

    public static MigrationBuilder CreateEnumTable<T>(this MigrationBuilder migrationBuilder)
        where T : struct, Enum
        => Sql(migrationBuilder, EnumTableSql.CreateTable<T>());

    public static MigrationBuilder DeleteEnumValues<T>(this MigrationBuilder migrationBuilder, params T[] values)
        where T : struct, Enum
        => Sql(migrationBuilder, EnumTableSql.Delete(values));

    /// <summary>
    /// Drops the column's foreign key to the enum table. Call this before
    /// <see cref="DropEnumTable{T}"/>, which fails while anything still references the table.
    /// </summary>
    public static MigrationBuilder DropEnumForeignKey<T>(
        this MigrationBuilder migrationBuilder, string table, string column)
        where T : struct, Enum
        => Sql(migrationBuilder, EnumTableSql.DropForeignKey<T>(table, column));

    public static MigrationBuilder DropEnumTable<T>(this MigrationBuilder migrationBuilder)
        where T : struct, Enum
        => Sql(migrationBuilder, EnumTableSql.DropTable<T>());

    public static MigrationBuilder InsertAllEnumValues<T>(this MigrationBuilder migrationBuilder)
        where T : struct, Enum
        => Sql(migrationBuilder, EnumTableSql.InsertAll<T>());

    public static MigrationBuilder InsertEnumValues<T>(this MigrationBuilder migrationBuilder, params T[] values)
        where T : struct, Enum
        => Sql(migrationBuilder, EnumTableSql.Insert(values));

    private static MigrationBuilder Sql(MigrationBuilder migrationBuilder, string sql)
    {
        if (string.IsNullOrEmpty(sql))
        {
            return migrationBuilder;
        }

        migrationBuilder.Sql(sql);
        return migrationBuilder;
    }
}

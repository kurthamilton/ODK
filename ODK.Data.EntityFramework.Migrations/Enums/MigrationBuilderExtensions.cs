using Microsoft.EntityFrameworkCore.Migrations;

namespace ODK.Data.EntityFramework.Migrations.Enums;

/// <summary>
/// Runs the <see cref="EnumTableSql"/> statements from a migration. Dropping the foreign key again
/// has no helper here - <see cref="MigrationBuilder.DropForeignKey"/> already covers it.
/// </summary>
public static class MigrationBuilderExtensions
{
    public static void AddEnumForeignKey<T>(this MigrationBuilder migrationBuilder, string table, string column)
        where T : struct, Enum
        => migrationBuilder.Sql(EnumTableSql.AddForeignKey<T>(table, column));

    public static void CreateEnumTable<T>(this MigrationBuilder migrationBuilder)
        where T : struct, Enum
        => migrationBuilder.Sql(EnumTableSql.CreateTable<T>());

    public static void DeleteEnumValues<T>(this MigrationBuilder migrationBuilder, params T[] values)
        where T : struct, Enum
        => Sql(migrationBuilder, EnumTableSql.Delete(values));

    public static void DropEnumTable<T>(this MigrationBuilder migrationBuilder)
        where T : struct, Enum
        => migrationBuilder.Sql(EnumTableSql.DropTable<T>());

    public static void InsertAllEnumValues<T>(this MigrationBuilder migrationBuilder)
        where T : struct, Enum
        => Sql(migrationBuilder, EnumTableSql.InsertAll<T>());

    public static void InsertEnumValues<T>(this MigrationBuilder migrationBuilder, params T[] values)
        where T : struct, Enum
        => Sql(migrationBuilder, EnumTableSql.Insert(values));

    private static void Sql(MigrationBuilder migrationBuilder, string sql)
    {
        if (string.IsNullOrEmpty(sql))
        {
            return;
        }

        migrationBuilder.Sql(sql);
    }
}

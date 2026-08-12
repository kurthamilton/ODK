using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Features;
using ODK.Data.EntityFramework.Migrations.Enums;

namespace ODK.Data.EntityFramework.Migrations.Tests.Enums;

[Parallelizable]
public class EnumTableSqlTests
{
    [Test]
    public void AddForeignKey_ReturnsStatementGuardedOnTheRelationship()
    {
        // Act
        var result = EnumTableSql.AddForeignKey<SiteFeatureType>("SiteSubscriptionFeatures", "SiteFeatureId");

        // Assert
        result.Should().Be(Lines(
            "IF NOT EXISTS (",
            "    SELECT 1",
            "    FROM sys.foreign_keys fk",
            "    INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id",
            "    WHERE fk.parent_object_id = OBJECT_ID(N'SiteSubscriptionFeatures')",
            "        AND fk.referenced_object_id = OBJECT_ID(N'SiteFeatures')",
            "        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = N'SiteFeatureId')",
            "BEGIN",
            "    ALTER TABLE [SiteSubscriptionFeatures] ADD CONSTRAINT [FK_SiteSubscriptionFeatures_SiteFeatures_SiteFeatureId]",
            "        FOREIGN KEY ([SiteFeatureId]) REFERENCES [SiteFeatures] ([Id]);",
            "END"));
    }

    [Test]
    public void CreateTable_ReturnsStatementGuardedOnTheTableNotExisting()
    {
        // Act
        var result = EnumTableSql.CreateTable<SiteFeatureType>();

        // Assert
        result.Should().Be(Lines(
            "IF OBJECT_ID(N'SiteFeatures', N'U') IS NULL",
            "BEGIN",
            "    CREATE TABLE [SiteFeatures] (",
            "        [Id] int NOT NULL,",
            "        [Name] nvarchar(100) NOT NULL,",
            "        CONSTRAINT [PK_SiteFeatures] PRIMARY KEY ([Id]),",
            "        CONSTRAINT [UQ_SiteFeatures_Name] UNIQUE ([Name])",
            "    );",
            "END"));
    }

    [Test]
    public void Delete_MultipleValues_ReturnsSingleStatementForAllIds()
    {
        // Act
        var result = EnumTableSql.Delete(SiteFeatureType.Theme, SiteFeatureType.Payments);

        // Assert
        result.Should().Be("DELETE FROM [SiteFeatures] WHERE [Id] IN (9, 6);");
    }

    [Test]
    public void Delete_NoValues_ReturnsEmpty()
    {
        // Act
        var result = EnumTableSql.Delete<SiteFeatureType>();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void DropForeignKey_ReturnsStatementFindingTheConstraintByRelationship()
    {
        // Arrange - the constraint name is not assumed, because one added by hand can be called anything.
        // Act
        var result = EnumTableSql.DropForeignKey<SiteFeatureType>("SiteSubscriptionFeatures", "SiteFeatureId");

        // Assert
        result.Should().Be(Lines(
            "DECLARE @name sysname;",
            "",
            "WHILE 1 = 1",
            "BEGIN",
            "    SET @name = (",
            "        SELECT TOP 1 fk.name",
            "        FROM sys.foreign_keys fk",
            "        INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id",
            "        WHERE fk.parent_object_id = OBJECT_ID(N'SiteSubscriptionFeatures')",
            "            AND fk.referenced_object_id = OBJECT_ID(N'SiteFeatures')",
            "            AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = N'SiteFeatureId');",
            "",
            "    IF @name IS NULL BREAK;",
            "",
            "    EXEC(N'ALTER TABLE [SiteSubscriptionFeatures] DROP CONSTRAINT ' + QUOTENAME(@name));",
            "END"));
    }

    [Test]
    public void DropForeignKey_MatchesTheSameRelationshipAsAddForeignKey()
    {
        // Arrange - the pair has to agree on what counts as this column's foreign key to the enum table.
        // If the drop matched anything narrower, a constraint the add declined to duplicate would be one
        // the drop then left in place, and the table drop after it would fail.
        var add = EnumTableSql.AddForeignKey<SiteFeatureType>("SiteSubscriptionFeatures", "SiteFeatureId");

        // Act
        var drop = EnumTableSql.DropForeignKey<SiteFeatureType>("SiteSubscriptionFeatures", "SiteFeatureId");

        // Assert - compared with indentation removed, since the two nest the clauses at different depths.
        Clauses(drop).Should().Contain(Clauses(add));
    }

    [Test]
    public void DropTable_ReturnsGuardedDropStatement()
    {
        // Act
        var result = EnumTableSql.DropTable<SiteFeatureType>();

        // Assert
        result.Should().Be("DROP TABLE IF EXISTS [SiteFeatures];");
    }

    [Test]
    public void Insert_MultipleValues_ReturnsAStatementPerValue()
    {
        // Act
        var result = EnumTableSql.Insert(SiteFeatureType.Theme, SiteFeatureType.InstagramFeed);

        // Assert
        result.Should().Be(Lines(
            "IF NOT EXISTS (SELECT 1 FROM [SiteFeatures] WHERE [Id] = 9)",
            "    INSERT INTO [SiteFeatures] ([Id], [Name]) VALUES (9, N'Custom theme');",
            "IF NOT EXISTS (SELECT 1 FROM [SiteFeatures] WHERE [Id] = 4)",
            "    INSERT INTO [SiteFeatures] ([Id], [Name]) VALUES (4, N'Instagram feed');"));
    }

    [Test]
    public void Insert_NoValues_ReturnsEmpty()
    {
        // Act
        var result = EnumTableSql.Insert<SiteFeatureType>();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Insert_ValueWithDisplayName_UsesTheDisplayName()
    {
        // Act
        var result = EnumTableSql.Insert(SiteFeatureType.AdminMembers);

        // Assert
        result.Should().Contain("VALUES (1, N'Make other members admins');");
    }

    [Test]
    public void Insert_ValueWithoutDisplayName_UsesTheMemberName()
    {
        // Act
        var result = EnumTableSql.Insert(SiteFeatureType.Payments);

        // Assert
        result.Should().Contain("VALUES (6, N'Payments');");
    }

    [Test]
    public void InsertAll_ReturnsAStatementForEveryValueExceptZero()
    {
        // Arrange
        var expected = Enum.GetValues<SiteFeatureType>()
            .Where(x => x != SiteFeatureType.None)
            .Select(x => $"WHERE [Id] = {(int)x})");

        // Act
        var result = EnumTableSql.InsertAll<SiteFeatureType>();

        // Assert
        result.Should().ContainAll(expected);
        result.Should().NotContain("WHERE [Id] = 0)");
    }

    /* The lines that decide which foreign key is matched, stripped of indentation and of the trailing
       bracket each caller closes its own subquery with. */
    private static string[] Clauses(string sql) => sql
        .Split(Environment.NewLine)
        .Select(x => x.Trim().TrimEnd(')', ';'))
        .Where(x => x.StartsWith("FROM sys.") || x.StartsWith("INNER JOIN sys.") ||
            x.StartsWith("WHERE fk.") || x.StartsWith("AND "))
        .ToArray();

    private static string Lines(params string[] lines) => string.Join(Environment.NewLine, lines);
}

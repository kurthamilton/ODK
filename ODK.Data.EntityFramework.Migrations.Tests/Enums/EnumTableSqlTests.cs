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
            "        FOREIGN KEY ([SiteFeatureId]) REFERENCES [SiteFeatures] ([SiteFeatureId]);",
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
            "        [SiteFeatureId] int NOT NULL,",
            "        [Name] nvarchar(100) NOT NULL,",
            "        CONSTRAINT [PK_SiteFeatures] PRIMARY KEY ([SiteFeatureId]),",
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
        result.Should().Be("DELETE FROM [SiteFeatures] WHERE [SiteFeatureId] IN (9, 6);");
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
            "IF NOT EXISTS (SELECT 1 FROM [SiteFeatures] WHERE [SiteFeatureId] = 9)",
            "    INSERT INTO [SiteFeatures] ([SiteFeatureId], [Name]) VALUES (9, N'Custom theme');",
            "IF NOT EXISTS (SELECT 1 FROM [SiteFeatures] WHERE [SiteFeatureId] = 4)",
            "    INSERT INTO [SiteFeatures] ([SiteFeatureId], [Name]) VALUES (4, N'Instagram feed');"));
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
            .Select(x => $"WHERE [SiteFeatureId] = {(int)x})");

        // Act
        var result = EnumTableSql.InsertAll<SiteFeatureType>();

        // Assert
        result.Should().ContainAll(expected);
        result.Should().NotContain("WHERE [SiteFeatureId] = 0)");
    }

    private static string Lines(params string[] lines) => string.Join(Environment.NewLine, lines);
}

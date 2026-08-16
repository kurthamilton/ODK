using FluentAssertions;
using NUnit.Framework;

namespace ODK.Data.EntityFramework.Migrations.Tests;

[Parallelizable]
public class ForeignKeySqlTests
{
    [Test]
    public void Drop_MatchesOnTheColumnRatherThanTheConstraintName()
    {
        // Act
        var result = ForeignKeySql.Drop("Chapters", "CountryId");

        // Assert - nothing here names a constraint, which is the point: the name is whatever the database
        // happens to hold, and for anything created outside EF that is not the name the scaffolder guesses.
        result.Should().Be(Lines(
            "DECLARE @name_Chapters_CountryId sysname;",
            "DECLARE @sql_Chapters_CountryId nvarchar(max);",
            "",
            "WHILE 1 = 1",
            "BEGIN",
            "    SET @name_Chapters_CountryId = (",
            "        SELECT TOP 1 fk.name",
            "        FROM sys.foreign_keys fk",
            "        INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id",
            "        WHERE fk.parent_object_id = OBJECT_ID(N'Chapters')",
            "            AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = N'CountryId');",
            "",
            "    IF @name_Chapters_CountryId IS NULL BREAK;",
            "",
            "    SET @sql_Chapters_CountryId = N'ALTER TABLE [Chapters] DROP CONSTRAINT ' + QUOTENAME(@name_Chapters_CountryId);",
            "    EXEC(@sql_Chapters_CountryId);",
            "END"));
    }

    [Test]
    public void Drop_LoopsSoASecondForeignKeyOnTheColumnCannotSurvive()
    {
        // Arrange - a column can carry more than one, and dropping only the first leaves the migration to
        // fail later on the one still there.

        // Act
        var result = ForeignKeySql.Drop("Payments", "CurrencyId");

        // Assert
        result.Should().Contain("WHILE 1 = 1");
        result.Should().Contain("IF @name_Payments_CurrencyId IS NULL BREAK;");
    }

    [Test]
    public void Drop_EscapesIdentifiersForBothTheStatementAndTheStringItSitsIn()
    {
        // Arrange - the table name reaches SQL twice over: once as an identifier, and once inside the string
        // literal that gets executed.

        // Act
        var result = ForeignKeySql.Drop("Odd]Name", "It's");

        // Assert
        result.Should().Contain("OBJECT_ID(N'Odd]Name')");
        result.Should().Contain("= N'It''s')");
        result.Should().Contain("N'ALTER TABLE [Odd]]Name] DROP CONSTRAINT '");

        // And the variable names drop anything that would not parse as one.
        result.Should().Contain("DECLARE @name_OddName_Its sysname;");
    }

    [Test]
    public void Drop_NamesItsVariablesAfterTheColumnSoTwoBlocksCanShareABatch()
    {
        /* Arrange - the generated script runs every statement of a migration in one batch, and variables are
           scoped to the batch rather than the block, so a migration dropping keys from several tables would
           declare the same name twice and fail. */

        // Act
        var first = ForeignKeySql.Drop("Payments", "CurrencyId");
        var second = ForeignKeySql.Drop("Countries", "CurrencyId");

        // Assert
        first.Should().Contain("@name_Payments_CurrencyId");
        second.Should().Contain("@name_Countries_CurrencyId");
    }

    [Test]
    public void Drop_BuildsTheStatementIntoAVariableRatherThanCallingAFunctionInsideExec()
    {
        // Arrange - EXEC takes string literals and variables joined by +, and nothing else, so QUOTENAME
        // inside it is "Incorrect syntax near 'QUOTENAME'" rather than a working drop.

        // Act
        var result = ForeignKeySql.Drop("Chapters", "CountryId");

        // Assert
        result.Should().Contain("EXEC(@sql_Chapters_CountryId);");
        result.Should().NotContain("EXEC(N'");
    }

    private static string Lines(params string[] lines) => string.Join(Environment.NewLine, lines);
}

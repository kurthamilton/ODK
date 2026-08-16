using FluentAssertions;
using NUnit.Framework;

namespace ODK.Data.EntityFramework.Migrations.Tests;

[Parallelizable]
public class IndexSqlTests
{
    [Test]
    public void Drop_MatchesOnTheColumnRatherThanTheIndexName()
    {
        // Act
        var result = IndexSql.Drop("Events", "VenueId");

        // Assert - nothing here names an index, which is the point: an index the database already has may
        // carry any name, and the scaffolder only ever emits the one EF would have chosen.
        result.Should().Contain("OBJECT_ID(N'Events')");
        result.Should().Contain("COL_NAME(ic.object_id, ic.column_id) = N'VenueId'");
        result.Should().NotContain("IX_Events_VenueId");
    }

    [Test]
    public void Drop_LeavesAnythingThatIsNotADuplicateOfWhatEfWouldCreate()
    {
        /* Arrange - only an index that duplicates a plain single-column one goes. A composite index that
           happens to start with the column serves queries this knows nothing about, a unique index is a
           constraint rather than a lookup aid, and dropping a clustered index would rewrite the table. */

        // Act
        var result = IndexSql.Drop("Events", "VenueId");

        // Assert
        result.Should().Contain("i.is_primary_key = 0");
        result.Should().Contain("i.is_unique = 0");
        result.Should().Contain("i.type_desc = N'NONCLUSTERED'");

        // Keyed on exactly one column, counting the key and not the included columns.
        result.Should().Contain("ic.is_included_column = 0) = 1");
    }

    [Test]
    public void Drop_BuildsTheStatementIntoAVariableRatherThanCallingAFunctionInsideExec()
    {
        // Arrange - EXEC takes string literals and variables joined by +, and nothing else, so QUOTENAME
        // inside it is "Incorrect syntax near 'QUOTENAME'" rather than a working drop.

        // Act
        var result = IndexSql.Drop("Events", "VenueId");

        // Assert
        result.Should().Contain("SET @sqlix_Events_VenueId = N'DROP INDEX ' + QUOTENAME(@index_Events_VenueId) + N' ON [Events]';");
        result.Should().Contain("EXEC(@sqlix_Events_VenueId);");
    }

    [Test]
    public void Drop_NamesItsVariablesAfterTheColumnSoTwoBlocksCanShareABatch()
    {
        // Arrange - the generated script runs a migration's statements in one batch, and variables are scoped
        // to the batch rather than the block.

        // Act
        var first = IndexSql.Drop("Events", "VenueId");
        var second = IndexSql.Drop("Events", "ChapterId");

        // Assert
        first.Should().Contain("@index_Events_VenueId");
        second.Should().Contain("@index_Events_ChapterId");
    }

    [Test]
    public void Drop_DoesNotCollideWithTheForeignKeyHelpersVariables()
    {
        // Arrange - a migration dropping both a foreign key and an index on the same column emits both
        // blocks into one batch, so their variable names have to differ.

        // Act
        var index = IndexSql.Drop("Events", "VenueId");
        var foreignKey = ForeignKeySql.Drop("Events", "VenueId");

        // Assert
        index.Should().NotContain("@name_Events_VenueId ");
        foreignKey.Should().NotContain("@index_Events_VenueId ");
    }
}

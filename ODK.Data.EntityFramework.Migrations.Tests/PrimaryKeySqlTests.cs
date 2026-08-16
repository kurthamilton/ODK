using FluentAssertions;
using NUnit.Framework;

namespace ODK.Data.EntityFramework.Migrations.Tests;

[Parallelizable]
public class PrimaryKeySqlTests
{
    [Test]
    public void Drop_FindsTheConstraintByTableRatherThanByName()
    {
        // Act
        var result = PrimaryKeySql.Drop("SentEmailEvents");

        // Assert - nothing here names a constraint. The scaffolder can only emit the name EF would have
        // chosen, which is no help against one created by hand.
        result.Should().Contain("FROM sys.key_constraints kc");
        result.Should().Contain("WHERE kc.parent_object_id = OBJECT_ID(N'SentEmailEvents')");
        result.Should().Contain("AND kc.type = 'PK'");
        result.Should().NotContain("PK_SentEmailEvents");
    }

    [Test]
    public void Drop_DoesNothingWhereTheTableHasNoPrimaryKey()
    {
        /* Arrange - a model mapped with HasKey does not mean the constraint is in the database.
           SentEmailEvents had none, and the scaffolded drop failed the whole migration. */

        // Act
        var result = PrimaryKeySql.Drop("SentEmailEvents");

        // Assert
        result.Should().Contain("IF @pk_SentEmailEvents IS NOT NULL");
    }

    [Test]
    public void Drop_BuildsTheStatementIntoAVariableRatherThanCallingAFunctionInsideExec()
    {
        // Arrange - EXEC takes string literals and variables joined by +, and nothing else.

        // Act
        var result = PrimaryKeySql.Drop("SentEmailEvents");

        // Assert
        result.Should().Contain(
            "SET @sqlpk_SentEmailEvents = N'ALTER TABLE [SentEmailEvents] DROP CONSTRAINT ' + QUOTENAME(@pk_SentEmailEvents);");
        result.Should().Contain("EXEC(@sqlpk_SentEmailEvents);");
    }

    [Test]
    public void Drop_NamesItsVariablesAfterTheTableSoSeveralCanShareABatch()
    {
        // Arrange - a migration re-keying several tables emits one of these per table, all into one batch,
        // where variables are scoped to the batch rather than the block.

        // Act
        var first = PrimaryKeySql.Drop("SentEmails");
        var second = PrimaryKeySql.Drop("SentEmailEvents");

        // Assert
        first.Should().Contain("@pk_SentEmails ");
        second.Should().Contain("@pk_SentEmailEvents ");
    }

    [Test]
    public void Drop_DoesNotLoop()
    {
        // Arrange - unlike the foreign key and index helpers: a table has at most one primary key, so there
        // is nothing to iterate.

        // Act
        var result = PrimaryKeySql.Drop("SentEmailEvents");

        // Assert
        result.Should().NotContain("WHILE 1 = 1");
    }
}

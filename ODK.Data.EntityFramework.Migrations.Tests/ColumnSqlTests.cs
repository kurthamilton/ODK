using FluentAssertions;
using NUnit.Framework;

namespace ODK.Data.EntityFramework.Migrations.Tests;

[Parallelizable]
public class ColumnSqlTests
{
    [Test]
    public void Drop_ClearsTheDefaultConstraintBeforeDroppingTheColumn()
    {
        /* Arrange - a default constraint on the column blocks the drop, and nothing about the failure says so
           until it happens. */

        // Act
        var result = ColumnSql.Drop("Payments", "PaymentReconciliationId");

        // Assert
        var constraintDrop = result.IndexOf("EXEC(@sqldefault_Payments_PaymentReconciliationId);");
        var columnDrop = result.IndexOf("DROP COLUMN [PaymentReconciliationId];");
        constraintDrop.Should().BeGreaterThan(-1);
        columnDrop.Should().BeGreaterThan(constraintDrop);
    }

    [Test]
    public void Drop_FindsTheDefaultConstraintByColumnRatherThanByName()
    {
        // Act
        var result = ColumnSql.Drop("Payments", "PaymentReconciliationId");

        // Assert - nothing here names a constraint. Whoever created the column named its default, and the
        // scaffolder can only emit the name EF would have chosen.
        result.Should().Contain("FROM sys.default_constraints dc");
        result.Should().Contain("WHERE dc.parent_object_id = OBJECT_ID(N'Payments')");
        result.Should().Contain("AND c.name = N'PaymentReconciliationId')");
        result.Should().NotContain("DF_");
    }

    [Test]
    public void Drop_DoesNothingWhereTheTableHasNoSuchColumn()
    {
        /* Arrange - the column this is for exists only in a restored database, so the same migration has to be
           a no-op against one built from the migrations alone. */

        // Act
        var result = ColumnSql.Drop("Payments", "PaymentReconciliationId");

        // Assert
        result.Should().Contain("IF COL_LENGTH(N'Payments', N'PaymentReconciliationId') IS NOT NULL");
        result.Should().Contain("IF @default_Payments_PaymentReconciliationId IS NOT NULL");
    }

    [Test]
    public void Drop_BuildsTheStatementIntoAVariableRatherThanCallingAFunctionInsideExec()
    {
        // Arrange - EXEC takes string literals and variables joined by +, and nothing else.

        // Act
        var result = ColumnSql.Drop("Payments", "PaymentReconciliationId");

        // Assert
        result.Should().Contain(
            "SET @sqldefault_Payments_PaymentReconciliationId = N'ALTER TABLE [Payments] DROP CONSTRAINT ' + " +
            "QUOTENAME(@default_Payments_PaymentReconciliationId);");
        result.Should().Contain("EXEC(@sqldefault_Payments_PaymentReconciliationId);");
    }

    [Test]
    public void Drop_NamesItsVariablesAfterTheTableAndColumnSoSeveralCanShareABatch()
    {
        // Arrange - a migration dropping several columns emits one of these per column, all into one batch,
        // where variables are scoped to the batch rather than the block.

        // Act
        var first = ColumnSql.Drop("Payments", "PaymentReconciliationId");
        var second = ColumnSql.Drop("Payments", "ExternalId");

        // Assert
        first.Should().Contain("@default_Payments_PaymentReconciliationId ");
        second.Should().Contain("@default_Payments_ExternalId ");
    }

    [Test]
    public void Drop_LeavesIndexesAndForeignKeysAlone()
    {
        /* Arrange - both also block a column drop, and both are the caller's to clear. Removing every index
           covering a column is a broader act than removing the column, so the error is the better outcome. */

        // Act
        var result = ColumnSql.Drop("Payments", "PaymentReconciliationId");

        // Assert
        result.Should().NotContain("sys.indexes");
        result.Should().NotContain("sys.foreign_keys");
    }
}

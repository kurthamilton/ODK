using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Data.EntityFramework;

namespace ODK.Services.Tests.Data;

/// <summary>
/// Pins the one property the generator exists for. Nothing else can catch it going: an id that is merely
/// random still inserts, still reads back, and fails no other test - it just scatters writes through every
/// clustered index in the database instead of appending to them.
/// </summary>
[Parallelizable]
public static class SequentialIdGeneratorTests
{
    [Test]
    public static void Next_AcrossInstances_ContinuesOneSequence()
    {
        /* Arrange - the repository base classes reach the sequence through a static entry point while a
           service reaches it through an injected instance. Both must be the same sequence: two counters
           would give two interleaved runs, and neither would ascend. */
        var first = new SequentialIdGenerator();
        var second = new SequentialIdGenerator();

        // Act - alternate between them.
        var ids = new[] { first.Next(), second.Next(), first.Next(), second.Next() };

        // Assert
        SortKeys(ids).Should().BeInAscendingOrder();
    }

    [Test]
    public static void Next_SuccessiveCalls_AscendInSqlServerSortOrder()
    {
        // Arrange
        var generator = new SequentialIdGenerator();

        // Act
        var ids = Enumerable.Range(0, 25).Select(_ => generator.Next()).ToArray();

        // Assert - non-empty first, because the generator is handed a null EntityEntry that the EF version
        // in use ignores. An upgrade that starts reading it throws here rather than on a live insert.
        ids.Should().OnlyContain(x => x != Guid.Empty);
        ids.Should().OnlyHaveUniqueItems();
        SortKeys(ids).Should().BeInAscendingOrder();
    }

    /// <summary>
    /// Rewrites each id into the byte order SQL Server compares <c>uniqueidentifier</c> in, so ordinary
    /// string ordering over the result says what the database would say. It is not byte order: the last six
    /// bytes are compared first, then 8-9, 6-7, 4-5 and finally 0-3.
    /// </summary>
    private static string[] SortKeys(Guid[] ids)
    {
        int[] sqlServerByteOrder = [10, 11, 12, 13, 14, 15, 8, 9, 6, 7, 4, 5, 0, 1, 2, 3];

        return ids
            .Select(id => id.ToByteArray())
            .Select(bytes => string.Concat(sqlServerByteOrder.Select(i => bytes[i].ToString("x2"))))
            .ToArray();
    }
}

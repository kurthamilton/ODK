using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Data;

/// <summary>
/// Covers the copy a column rename in flight depends on. Both columns exist for three deploys, and every
/// write has to reach both: the readers move across while the previous build is still writing, so a value
/// that reached only one of them is lost at that point rather than at the point it was written.
/// </summary>
/// <remarks>
/// <c>ChapterTexts.DescriptionHtml</c> stands in for all seventeen - they are declared the same way, by
/// <c>EntityTypeBuilderExtensions.DualWriteColumn</c>, and the interceptor reads the pairing off the model
/// rather than from a list of its own.
/// </remarks>
[Parallelizable]
public static class HtmlColumnMirrorInterceptorTests
{
    private const string Mirror = "DescriptionHtmlMirror";

    [Test]
    public static async Task SaveChanges_EntityAttachedAsModified_WritesTheMirror()
    {
        /* Arrange - an update as the repositories do it. Queries are untracked, so the entity is attached
           whole and every property is marked modified, the copy included: left alone it is written as null,
           which would empty the new column for every row anyone edits. */
        using var context = new MockOdkContext();
        var chapterId = Guid.NewGuid();

        context.Add(new ChapterTexts { ChapterId = chapterId, DescriptionHtml = "<p>Stored</p>" });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var edited = new ChapterTexts { ChapterId = chapterId, DescriptionHtml = "<p>Edited</p>" };

        // Act
        context.Attach(edited);
        context.Entry(edited).State = EntityState.Modified;
        await context.SaveChangesAsync();

        // Assert
        StoredMirror(context).Should().Be("<p>Edited</p>");
    }

    [Test]
    public static async Task SaveChanges_InsertedEntity_WritesTheMirror()
    {
        // Arrange
        using var context = new MockOdkContext();

        // Act
        context.Add(new ChapterTexts { ChapterId = Guid.NewGuid(), DescriptionHtml = "<p>Inserted</p>" });
        await context.SaveChangesAsync();

        // Assert
        StoredMirror(context).Should().Be("<p>Inserted</p>");
    }

    /// <summary>Reads the copy back off a fresh entry, so what is asserted is what was stored.</summary>
    private static string? StoredMirror(MockOdkContext context)
    {
        context.ChangeTracker.Clear();

        var stored = context.Set<ChapterTexts>().Single();
        return (string?)context.Entry(stored).Property(Mirror).CurrentValue;
    }
}

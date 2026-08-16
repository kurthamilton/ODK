using System;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ODK.Core;
using ODK.Data.EntityFramework.Interceptors;
using ODK.Data.EntityFramework.Mapping;

namespace ODK.Services.Tests.Data;

/// <summary>
/// The transitional dual-write behind renaming primary key columns to <c>Id</c> - see
/// <see cref="IdColumnRename"/>.
/// </summary>
/// <remarks>
/// Against a model of its own rather than the app's: which tables are part-way through a rename changes with
/// every batch, and between batches there is none at all, so a test pinned to a real entity would have to be
/// repointed or deleted each time. What has to keep working is the interceptor.
/// </remarks>
[Parallelizable]
public static class IdColumnRenameTests
{
    [Test]
    public static void SaveChanges_EntityWithARenamedIdColumn_WritesTheKeyToTheNewColumnToo()
    {
        /* Arrange - nothing at the call site writes the new column, so nothing at the call site fails when it
           stops being written. The migration that turns the column into the key relies on the build before it
           having filled the column for every row that build inserted. */
        using var context = new RenamingContext(renaming: true);
        var entity = new RenamingEntity { Id = Guid.NewGuid() };
        context.Add(entity);

        // Act
        context.SaveChanges();

        // Assert
        context.Entry(entity).Property(IdColumnRename.ShadowPropertyName).CurrentValue
            .Should().Be(entity.Id);
    }

    [Test]
    public static void SaveChanges_EntityWithoutARenamedIdColumn_SavesUntouched()
    {
        // Arrange - only the maps that opt in carry the property, so a save of anything else has to pass
        // straight through rather than the interceptor tripping over a property that is not there.
        using var context = new RenamingContext(renaming: false);
        context.Add(new RenamingEntity { Id = Guid.NewGuid() });

        // Act
        var act = () => context.SaveChanges();

        // Assert
        act.Should().NotThrow();
    }

    private class RenamingEntity : IDatabaseEntity
    {
        public Guid Id { get; set; }
    }

    private class RenamingContext : DbContext
    {
        private readonly bool _renaming;

        public RenamingContext(bool renaming)
        {
            _renaming = renaming;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options
            .UseInMemoryDatabase($"id-column-rename-{Guid.NewGuid()}")
            .AddInterceptors(new IdColumnRenameInterceptor());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<RenamingEntity>();
            builder.HasKey(x => x.Id);

            if (_renaming)
            {
                builder.HasRenamedIdColumn();
            }
        }
    }
}

using FluentAssertions;
using NUnit.Framework;
using ODK.Data.EntityFramework.Mapping;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Data;

/// <summary>
/// The transitional dual-write behind renaming primary key columns to <c>Id</c> - see
/// <see cref="IdColumnRename"/>.
/// </summary>
[Parallelizable]
public static class IdColumnRenameTests
{
    [Test]
    public static void SaveChanges_EntityWithARenamedIdColumn_WritesTheKeyToTheNewColumnToo()
    {
        /* Arrange - nothing at the call site writes the new column, so nothing at the call site fails when it
           stops being written. The migration that turns the column into the key relies on the build before it
           having filled the column for every row that build inserted, which is what this covers. */
        using var context = new MockOdkContext();
        var country = context.CreateCountry();

        // Act
        context.SaveChanges();

        // Assert
        context.Entry(country).Property(IdColumnRename.ShadowPropertyName).CurrentValue
            .Should().Be(country.Id);
    }

    [Test]
    public static void SaveChanges_EntityWithoutARenamedIdColumn_SavesUntouched()
    {
        // Arrange - only the maps that opt in carry the property, so a save of anything else has to pass
        // straight through rather than the interceptor tripping over a property that is not there.
        using var context = new MockOdkContext();
        var member = context.CreateMember();

        // Act
        var act = () => context.SaveChanges();

        // Assert
        act.Should().NotThrow();
        context.Entry(member).Metadata.FindProperty(IdColumnRename.ShadowPropertyName)
            .Should().BeNull();
    }
}

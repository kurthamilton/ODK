using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Features;
using ODK.Data.EntityFramework.Migrations.Enums;

namespace ODK.Data.EntityFramework.Migrations.Tests.Enums;

[Parallelizable]
public class EnumTablesTests
{
    [Test]
    public void Get_RegisteredType_ReturnsTable()
    {
        // Act
        var result = EnumTables.Get<SiteFeatureType>();

        // Assert
        result.Name.Should().Be("SiteFeatures");
        result.IdColumnName.Should().Be("Id");
    }

    [Test]
    public void Get_UnregisteredType_Throws()
    {
        // Act
        var act = () => EnumTables.Get(typeof(DayOfWeek));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*DayOfWeek*");
    }
}

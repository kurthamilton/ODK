using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.DataTypes;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
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
    public void Get_RegisteredTypes_MapToDistinctTables()
    {
        // Arrange - two types sharing a table would have each one's values inserted into the other's rows,
        // and is the way a registry written by copying an entry goes wrong.
        Type[] registered =
        [
            typeof(ChapterAdminRole),
            typeof(DataType),
            typeof(DistanceUnitType),
            typeof(EmailRecipientType),
            typeof(EventResponseType),
            typeof(MemberEmailPreferenceType),
            typeof(NotificationType),
            typeof(PlatformType),
            typeof(SiteFeatureType),
            typeof(SubscriptionType)
        ];

        // Act
        var names = registered.Select(x => EnumTables.Get(x).Name).ToArray();

        // Assert
        names.Should().OnlyHaveUniqueItems();
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

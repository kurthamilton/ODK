using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.DataTypes;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Features;
using ODK.Core.Issues;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;

namespace ODK.Data.EntityFramework.Migrations.Enums;

/// <summary>
/// The enum types that have a lookup table, and the table each one maps to.
/// </summary>
/// <remarks>
/// Registration is explicit and an unregistered type throws. Deriving a name from the type instead
/// (pluralise, drop a "Type" suffix) silently produces a plausible-looking name for a table that
/// doesn't exist, and the migration then creates a second, orphaned one alongside the real table.
/// </remarks>
public static class EnumTables
{
    private static readonly IReadOnlyDictionary<Type, EnumTable> Tables = new Dictionary<Type, EnumTable>
    {
        [typeof(ChapterAdminRole)] = new EnumTable
        {
            Name = "ChapterAdminRoles"
        },
        [typeof(DataType)] = new EnumTable
        {
            Name = "DataTypes"
        },
        [typeof(DistanceUnitType)] = new EnumTable
        {
            Name = "DistanceUnitTypes"
        },
        [typeof(EmailRecipientType)] = new EnumTable
        {
            Name = "EmailRecipientTypes"
        },
        [typeof(EventResponseType)] = new EnumTable
        {
            Name = "EventResponseTypes"
        },
        [typeof(IssueStatusType)] = new EnumTable
        {
            Name = "IssueStatusTypes"
        },
        [typeof(IssueType)] = new EnumTable
        {
            Name = "IssueTypes"
        },
        [typeof(MemberEmailPreferenceType)] = new EnumTable
        {
            Name = "MemberEmailPreferenceTypes"
        },
        [typeof(NotificationType)] = new EnumTable
        {
            Name = "NotificationTypes"
        },
        [typeof(PlatformType)] = new EnumTable
        {
            Name = "PlatformTypes"
        },
        [typeof(SiteFeatureType)] = new EnumTable
        {
            Name = "SiteFeatures"
        },
        [typeof(SubscriptionType)] = new EnumTable
        {
            Name = "SubscriptionTypes"
        }
    };

    public static EnumTable Get<T>()
        where T : struct, Enum
        => Get(typeof(T));

    public static EnumTable Get(Type enumType)
    {
        if (!Tables.TryGetValue(enumType, out var table))
        {
            throw new ArgumentException(
                $"No enum table is registered for {enumType.Name}. Add one to {nameof(EnumTables)}.",
                nameof(enumType));
        }

        return table;
    }
}

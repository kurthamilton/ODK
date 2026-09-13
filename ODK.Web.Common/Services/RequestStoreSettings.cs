using ODK.Core.Platforms;

namespace ODK.Web.Common.Services;

public class RequestStoreSettings
{
    public required EnvironmentType Environment { get; init; }

    /// <summary>
    /// How long a group counts as newly arrived (Groups:MigrationWindowDays): the window the moved page
    /// stays in the group admin menu for, and the same one the group's home page announces a move for.
    /// </summary>
    public required int MigrationWindowDays { get; init; }
}

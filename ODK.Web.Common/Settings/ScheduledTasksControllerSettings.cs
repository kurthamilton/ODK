namespace ODK.Web.Common.Settings;

/// <summary>
/// What <c>ScheduledTasksController</c> needs from <c>ScheduledTasks</c> configuration, mapped in
/// <c>DependencyRegistrar</c>.
/// </summary>
public class ScheduledTasksControllerSettings
{
    public required string ApiKey { get; init; }
}
